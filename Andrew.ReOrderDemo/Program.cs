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

            var ro = new StupidReOrderBufferDemo(TimeSpan.FromMilliseconds(100));
            foreach (var item in GetCommands(true))
            {
                ro.Push(item);
            }
            ro.Flush();
            ro.DumpMetrics(Console.Out);
        }


        static IEnumerable<OrderedCommand> GetCommands(bool boost = true)
        {
            int total_count = 3000;
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
                    OccurAt = start + TimeSpan.FromMilliseconds(cmd_noise + i * cmd_period + rnd.Next(cmd_noise) - cmd_noise / 2),
                    Message = $"CMD-{i:#00000}"
                });
            }

            foreach (var c in (from x in orders orderby x.OccurAt ascending select x))
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
        public DateTime OccurAt = DateTime.MinValue;
        public string Message;

        public override string ToString()
        {
            return $"{this.Position:#000}, {this.Message}; {this.OccurAt:HH:mm:ss.fff}";
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
            Console.WriteLine($"POP:  {data}");
            return true;
        }
        public bool Drop(OrderedCommand data, string reason)
        {
            this._metrics_total_drop++;
            Console.WriteLine($"DROP: {data} ({reason})");
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



    public class StupidReOrderBufferDemo : ReOrderBufferBase
    {
        private List<OrderedCommand> _buffer = new List<OrderedCommand>();

        public StupidReOrderBufferDemo(TimeSpan duration) : base(duration)
        {

        }

        protected override bool _Flush()
        {
            this._buffer.Sort(new OrderedCommandComparer());

            int index = 0;
            foreach(var x in this._buffer)
            {
                if (x.Position == index) this.Pop(x);
                else this.Drop(x, "unknown");

                index++;
            }

            return true;
        }

        protected override bool _Push(OrderedCommand data)
        {
            this._buffer.Add(data);
            return true;
        }
    }
}
