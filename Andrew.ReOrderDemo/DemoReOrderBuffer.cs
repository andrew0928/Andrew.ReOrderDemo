﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Andrew.ReOrderDemo
{
    public class DemoReOrderBuffer : IReOrderBuffer
    {
        private int _current_next_index = 0;
        private SortedSet<OrderedCommand> _buffer = new SortedSet<OrderedCommand>(new OrderedCommandComparer());

        private IReOrderBuffer _this_interface { get { return (IReOrderBuffer)this; } }


        protected readonly int _buffer_size = 0;
        protected readonly TimeSpan _buffer_duration = TimeSpan.Zero;

        private event CommandProcessEventHandler _pop;
        private event CommandProcessEventHandler _drop;

        public DemoReOrderBuffer(TimeSpan buffer_duration_limit, int buffer_size_limit)// : base()
        {
            this._buffer_duration = buffer_duration_limit;
            this._buffer_size = buffer_size_limit;
        }

        event CommandProcessEventHandler IReOrderBuffer.PopCommand
        {
            add => this._pop += value;
            remove => this._pop-= value;
        }

        event CommandProcessEventHandler IReOrderBuffer.DropCommand
        {
            add => this._drop += value;
            remove => this._drop -= value;
        }


        private int _metrics_total_push = 0;
        private int _metrics_total_pop = 0;
        private int _metrics_total_drop = 0;
        private int _metrics_buffer_max = 0;

        public (int push, int pop, int drop, int buffer_max) ResetMetrics()
        {
            return (
                Interlocked.Exchange(ref this._metrics_total_push, 0),
                Interlocked.Exchange(ref this._metrics_total_pop, 0),
                Interlocked.Exchange(ref this._metrics_total_drop, 0),
                Interlocked.Exchange(ref this._metrics_buffer_max, 0));
        }


        bool IReOrderBuffer.Push(OrderedCommand data)
        {
            this._metrics_total_push++;


            this._metrics_buffer_max = Math.Max(this._metrics_buffer_max, this._buffer.Count);

            if (data.Position < this._current_next_index)
            {
                // drop
                this.Drop(data, CommandProcessReasonEnum.DROP_WRONG_ORDER);
                return false;
            }
            else if (data.Position == this._current_next_index)
            {
                // pop series
                this.Pop(data, CommandProcessReasonEnum.POP_PASSTHRU);
                this._current_next_index = data.Position + 1;

                while (this._buffer.Count > 0 && this._buffer.Min.Position == this._current_next_index)
                {
                    var x = this._buffer.Min;
                    this._buffer.Remove(x);

                    this.Pop(x, CommandProcessReasonEnum.POP_BUFFERED);
                    this._current_next_index = x.Position + 1;
                }

                this._metrics_buffer_max = Math.Max(this._metrics_buffer_max, this._buffer.Count);
                return true;
            }
            else if (this._buffer.Count >= this._buffer_size)
            {
                this.Drop(data, CommandProcessReasonEnum.DROP_BUFFER_SIZE_FULL);
                return false;
            }
            else
            {
                // queued & refresh buffer
                this._buffer.Add(data);

                while (this._buffer.Count > 0 && (this._buffer.Max.OccurAt - this._buffer.Min.OccurAt) > this._buffer_duration)
                {
                    var m = this._buffer.Min;

                    if (m.Position == this._current_next_index)
                    {
                        // pop
                        this.Pop(m, CommandProcessReasonEnum.POP_BUFFERED);
                    }
                    else
                    {
                        // skip
                        this.Drop(m, CommandProcessReasonEnum.DROP_BUFFER_DURATION_FULL);
                    }

                    this._current_next_index = m.Position + 1;
                    this._buffer.Remove(m);
                }

                this._metrics_buffer_max = Math.Max(this._metrics_buffer_max, this._buffer.Count);
                return true;
            }
        }

        
        bool IReOrderBuffer.Flush()
        {
            while (this._buffer.Count > 0)
            {
                var m = this._buffer.Min;
                this.Drop(m, CommandProcessReasonEnum.DROP_FORCE_FLUSH);// "flush");

                this._buffer.Remove(m);
            }

            return true;
        }



        protected bool Pop(OrderedCommand data, CommandProcessReasonEnum reason)
        {
            this._metrics_total_pop++;
            //Console.WriteLine($"POP:  {data.Position:#000}, {data.Message};");

            this._pop?.Invoke(data, new CommandProcessEventArgs()
            {
                Result = CommandProcessResultEnum.POP,
                Reason = reason,
            });

            return true;
        }




        protected bool Drop(OrderedCommand data, CommandProcessReasonEnum reason)
        {
            this._metrics_total_drop++;
            //Console.WriteLine($"DROP: {data.Position:#000}, {data.Message}; ({reason})");

            this._drop?.Invoke(data, new CommandProcessEventArgs()
            {
                Result = CommandProcessResultEnum.DROP,
                Reason = reason,
            });

            return true;
        }
    }
}
