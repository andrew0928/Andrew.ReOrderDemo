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
        protected readonly int _buffer_size = 0;


        public ReOrderBuffer(int buffer_size_limit)
        {
            this._buffer_size = buffer_size_limit;
        }


        #region 實做 interface event 的必要轉接 code
        private event CommandProcessEventHandler _send;
        private event CommandProcessEventHandler _drop;
        private event CommandSkipEventHandler _skip;

        event CommandProcessEventHandler IReOrderBuffer.CommandIsReadyToSend
        {
            add => this._send += value;
            remove => this._send-= value;
        }

        event CommandProcessEventHandler IReOrderBuffer.CommandWasDroped
        {
            add => this._drop += value;
            remove => this._drop -= value;
        }

        event CommandSkipEventHandler IReOrderBuffer.CommandWasSkipped
        {
            add => this._skip += value;
            remove => this._skip -= value;
        }
        #endregion


        private int _metrics_total_push = 0;
        private int _metrics_total_send = 0;
        private int _metrics_total_drop = 0;
        private int _metrics_total_skip = 0;
        private int _metrics_buffer_max = 0;
        private double _metrics_buffer_delay = 0.0;

        public (int push, int send, int drop, int skip, int buffer_max, double delay) ResetMetrics()
        {
            return (
                Interlocked.Exchange(ref this._metrics_total_push, 0),
                Interlocked.Exchange(ref this._metrics_total_send, 0),
                Interlocked.Exchange(ref this._metrics_total_drop, 0),
                Interlocked.Exchange(ref this._metrics_total_skip, 0),
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
                this.Drop(data, CommandProcessReasonEnum.DROP_OUTOFORDER);
                return false;
            }
            else 
            {
                if (data.Position == this._current_next_index)
                {
                    this.Send(data, CommandProcessReasonEnum.SEND_PASSTHRU);
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
                        this.Skip(this._current_next_index, CommandProcessReasonEnum.SKIP_BUFFERFULL);
                        this._current_next_index++;
                    }
                    while (this._buffer.Count > 0 && this._current_next_index == this._buffer.Min.Position)
                    {
                        var m = this._buffer.Min;
                        this._buffer.Remove(m);
                        this.Send(m, CommandProcessReasonEnum.SEND_BUFFERED);
                        this._current_next_index++;
                    }
                } while (this._buffer.Count > this._buffer_size);

                this._metrics_buffer_max = Math.Max(this._metrics_buffer_max, this._buffer.Count);
                return true;
            }
        }

        
        bool IReOrderBuffer.Flush()
        {
            while(this._buffer.Count > 0)
            {
                if (this._current_next_index == this._buffer.Min.Position)
                {
                    // pop
                    var m = this._buffer.Min;
                    this._buffer.Remove(m);
                    this.Send(m, CommandProcessReasonEnum.SEND_BUFFERED);
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
                        CommandProcessReasonEnum.SKIP_BUFFERFULL);
                    this._current_next_index++;
                }
            }            

            return true;
        }



        protected bool Send(OrderedCommand data, CommandProcessReasonEnum reason)
        {
            this._metrics_buffer_delay += (DateTimeUtil.Instance.Now - data.Origin).TotalMilliseconds;// (this._metrics_average_latency * this._metrics_total_pop + (data.OccurAt - data.Origin).TotalMilliseconds) / (this._metrics_total_pop + 1);
            this._metrics_total_send++;
            this._send?.Invoke(data, new CommandProcessEventArgs()
            {
                Result = CommandProcessResultEnum.SEND,
                Reason = reason,
            });

            return true;
        }


        protected bool Drop(OrderedCommand data, CommandProcessReasonEnum reason)
        {
            this._metrics_total_drop++;
            this._drop?.Invoke(data, new CommandProcessEventArgs()
            {
                Result = CommandProcessResultEnum.DROP,
                Reason = reason,
            });

            return true;
        }

        protected bool Skip(int position, CommandProcessReasonEnum reason)
        {
            this._metrics_total_skip++;
            this._skip?.Invoke(position, new CommandProcessEventArgs()
            {
                Result = CommandProcessResultEnum.SKIP,
                Reason = reason,
            });

            return true;
        }

    }
}
