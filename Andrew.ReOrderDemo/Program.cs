using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.IO;

namespace Andrew.ReOrderDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //foreach(var x in GetCommands(false))
            //{
            //    Console.WriteLine($"{x}");
            //}
            //return;            
            var ro = new DemoReOrderBuffer(TimeSpan.FromMilliseconds(100));
            foreach(var item in GetCommands(true))
            {
                ro.Push(item);
            }
            ro.Flush();
            ro.DumpMetrics(Console.Out);
        }


        static IEnumerable<OrderedCommand> GetCommands(bool boost = true)
        {
            int total_count = 10000;
            int cmd_period = 100;
            int cmd_noise = 500;


            List<OrderedCommand> orders = new List<OrderedCommand>();
            DateTime start = DateTime.Now;

            Random rnd = new Random();
            for (int i = 0; i < total_count; i++)
            {
                orders.Add(new OrderedCommand()
                {
                    Position = i,
                    Origin = start + TimeSpan.FromMilliseconds(i * cmd_period),
                    OccurAt = start + TimeSpan.FromMilliseconds(i * cmd_period + rnd.Next(cmd_noise)),
                    Message = $"CMD-{i:#00000}"
                });
            }

            foreach(var c in (from x in orders orderby x.OccurAt ascending select x))
            {
                if (!boost)
                {
                    Task.Delay(c.OccurAt - start).Wait();
                    start = c.OccurAt;
                }
                yield return c;
            }
        }
    }


    public class OrderedCommand
    {
        public int Position = 0;
        public DateTime Origin = DateTime.MinValue;
        public DateTime OccurAt = DateTime.MinValue;
        public string Message;

        public override string ToString()
        {
            return $"{this.Position:#000}, {this.Message}; {this.OccurAt:HH:mm:ss.fff} (Delay: {(this.OccurAt - this.Origin).TotalMilliseconds}) msec";
        }
    }


    public abstract class ReOrderBufferBase
    {
        protected readonly TimeSpan _buffer_duration = TimeSpan.Zero;


        private int _metrics_total_push = 0;
        private int _metrics_total_pop = 0;
        private int _metrics_total_drop = 0;
        private int _metrics_buffer_max = 0;


        protected ReOrderBufferBase(TimeSpan buffer_duration_limit)
        {
            this._buffer_duration = buffer_duration_limit;
        }


        public bool Push(OrderedCommand data)
        {
            this._metrics_total_push++;

            int current_buffer_size = this._metrics_total_push - this._metrics_total_pop - this._metrics_total_drop;
            this._metrics_buffer_max = Math.Max(current_buffer_size, this._metrics_buffer_max);
            Console.WriteLine($"- PUSH: {data}");
            return this._Push(data);
        }
        public bool Flush()
        {
            return this._Flush();
        }

        protected abstract bool _Push(OrderedCommand data);
        protected abstract bool _Flush();

        public bool Pop(OrderedCommand data)
        {
            this._metrics_total_pop++;
            Console.WriteLine($"POP:  {data.Position:#000}, {data.Message};");
            return true;
        }
        public bool Drop(OrderedCommand data, string reason)
        {
            this._metrics_total_drop++;
            Console.WriteLine($"DROP: {data.Position:#000}, {data.Message}; ({reason})");
            return true;
        }


        public void DumpMetrics(TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine($"ReOrder Metrics:");
            writer.WriteLine($"- total push: {this._metrics_total_push}");
            writer.WriteLine($"- total pop:  {this._metrics_total_pop}");
            writer.WriteLine($"- total drop: {this._metrics_total_drop}");
            writer.WriteLine($"- drop rate: {this._metrics_total_drop * 100D / this._metrics_total_push:#.##} %");
            writer.WriteLine($"- max buffer size: {this._metrics_buffer_max}");
        }


        public class OrderedCommandComparer : IComparer<OrderedCommand>
        {
            public int Compare([AllowNull] OrderedCommand x, [AllowNull] OrderedCommand y)
            {
                return x.Position.CompareTo(y.Position);
            }
        }
    }


    public class DemoReOrderBuffer : ReOrderBufferBase
    {
        private int _current_next_index = 0;
        private SortedSet<OrderedCommand> _buffer = new SortedSet<OrderedCommand>(new ReOrderBufferBase.OrderedCommandComparer());

        public DemoReOrderBuffer(TimeSpan buffer_duration_limit) : base(buffer_duration_limit)
        {

        }

        protected override bool _Push(OrderedCommand data)
        {
            if (data.Position < this._current_next_index)
            {
                // drop
                this.Drop(data, "wrong order");
                return false;
            }
            else if (data.Position == this._current_next_index)
            {
                // pop series

                this.Pop(data);
                this._current_next_index = data.Position +1;

                while (this._buffer.Count > 0 && this._buffer.Min.Position == this._current_next_index)
                {
                    var x = this._buffer.Min;
                    this._buffer.Remove(x);

                    this.Pop(x);
                    this._current_next_index = x.Position + 1;
                }
                return true;
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
                        this.Pop(m);
                    }
                    else
                    {
                        // skip
                        this.Drop(m, "buffer full");
                    }

                    this._current_next_index = m.Position + 1;
                    this._buffer.Remove(m);
                }
                return true;
            }
        }

        protected override bool _Flush()
        {
            while(this._buffer.Count > 0)
            {
                var m = this._buffer.Min;
                this.Drop(m, "flush");

                this._buffer.Remove(m);
            }

            return true;
        }
    }

}















































