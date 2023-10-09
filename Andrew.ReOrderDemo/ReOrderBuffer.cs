using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Andrew.ReOrderDemo
{
    public class ReOrderBuffer : IReOrderBuffer
    {
        private int _current_next_index = 0;
        private SortedSet<OrderedCommand> _buffer = new SortedSet<OrderedCommand>(new OrderedCommandComparer());

        private IReOrderBuffer _this_interface { get { return (IReOrderBuffer)this; } }


        protected readonly int _buffer_size = 0;
        //protected readonly TimeSpan _command_max_delay = TimeSpan.Zero;

        private event CommandProcessEventHandler _pop;
        private event CommandProcessEventHandler _drop;

        public ReOrderBuffer(int buffer_size_limit)// : base()
        {
            //this._command_max_delay = command_delay_limit;
            this._buffer_size = buffer_size_limit;
        }

        event CommandProcessEventHandler IReOrderBuffer.CommandIsReadyToSend
        {
            add => this._pop += value;
            remove => this._pop-= value;
        }

        event CommandProcessEventHandler IReOrderBuffer.CommandWasDroped
        {
            add => this._drop += value;
            remove => this._drop -= value;
        }

        //event CommandProcessEventHandler IReOrderBuffer.CommandWasSkipped
        //{
        //    add => this._skip += value;
        //    remove => this._skip -= value;
        //}

        private int _metrics_total_push = 0;
        private int _metrics_total_pop = 0;
        private int _metrics_total_drop = 0;
        private int _metrics_buffer_max = 0;
        private double _metrics_buffer_delay = 0.0;

        public (int push, int pop, int drop, int buffer_max, double latency) ResetMetrics()
        {
            return (
                Interlocked.Exchange(ref this._metrics_total_push, 0),
                Interlocked.Exchange(ref this._metrics_total_pop, 0),
                Interlocked.Exchange(ref this._metrics_total_drop, 0),
                Interlocked.Exchange(ref this._metrics_buffer_max, 0),
                Interlocked.Exchange(ref this._metrics_buffer_delay, 0));
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
            else 
            {
                if (data.Position == this._current_next_index)
                {
                    this.Pop(data, CommandProcessReasonEnum.POP_PASSTHRU);
                    this._current_next_index = data.Position + 1;
                }
                else
                {
                    this._buffer.Add(data);
                }

                do
                {
                    if (this._buffer.Count > this._buffer_size && this._current_next_index < this._buffer.Min.Position)
                    {
                        // skip:
                        this.Drop(
                            new OrderedCommand()
                            {
                                Position = this._current_next_index,
                                Message = "Command not received, and skip waiting. Message body unknown."
                            },
                            CommandProcessReasonEnum.DROP_SKIPPED);
                        this._current_next_index++;
                    }
                    while (this._buffer.Count > 0 && this._current_next_index == this._buffer.Min.Position)
                    {
                        var m = this._buffer.Min;
                        this._buffer.Remove(m);
                        this.Pop(m, CommandProcessReasonEnum.POP_BUFFERED);
                        this._current_next_index++;
                    }
                } while (this._buffer.Count > this._buffer_size);



                this._metrics_buffer_max = Math.Max(this._metrics_buffer_max, this._buffer.Count);
                return true;
            }
        }

        
        bool IReOrderBuffer.Flush()
        {
            //while (this._buffer.Count > 0)
            //{
            //    var m = this._buffer.Min;
            //    this.Drop(m, CommandProcessReasonEnum.DROP_FORCE_FLUSH);// "flush");

            //    this._buffer.Remove(m);
            //}

            while(this._buffer.Count > 0)
            {
                if (this._current_next_index == this._buffer.Min.Position)
                {
                    // pop
                    var m = this._buffer.Min;
                    this._buffer.Remove(m);
                    this.Pop(m, CommandProcessReasonEnum.POP_BUFFERED);
                    this._current_next_index++;
                }
                else
                {
                    // skip
                    this.Drop(
                        new OrderedCommand()
                        {
                            Position = this._current_next_index,
                            Message = "Command not received, and skip waiting. Message body unknown."
                        },
                        CommandProcessReasonEnum.DROP_SKIPPED);
                    this._current_next_index++;
                }
            }
            

            return true;
        }



        protected bool Pop(OrderedCommand data, CommandProcessReasonEnum reason)
        {
            //if ((DateTimeUtil.Instance.Now - data.Origin) < this._command_max_delay)
            //if (true)
            {
                this._metrics_buffer_delay += (DateTimeUtil.Instance.Now - data.Origin).TotalMilliseconds;// (this._metrics_average_latency * this._metrics_total_pop + (data.OccurAt - data.Origin).TotalMilliseconds) / (this._metrics_total_pop + 1);
                this._metrics_total_pop++;

                //Console.WriteLine($"POP:  {data.Position:#000}, {data.Message}; ({reason})");
                this._pop?.Invoke(data, new CommandProcessEventArgs()
                {
                    Result = CommandProcessResultEnum.POP,
                    Reason = reason,
                });
            }
            //else
            //{
            //    //this._metrics_total_drop++;
            //    this.Drop(data, CommandProcessReasonEnum.DROP_COMMAND_EXPIRED);
            //}




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
