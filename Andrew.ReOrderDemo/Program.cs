using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Andrew.ReOrderDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach(var x in GetCommands())
            {
                Console.WriteLine($"{x.Position:#000}, {x.Message}; {x.OccurAt.TotalMilliseconds}");
            }
            return;


            var ro = new DemoReOrderBuffer(3);

            foreach(var item in GetData())
            {
                ro.Push(item);
            }
        }

        static IEnumerable<int> GetData()
        {
            return new int[] { 3, 2, 1, 0, 4, 5, 9, 6, 7, 10, 8, 11, 12, 20, 19, 13, 14, 18, 17, 16, 15,30,29,28,27,26,25,24,23,22,21 };
        }

        static IEnumerable<OrderedCommand> GetCommands(bool boost = true)
        {
            //SortedSet<OrderedCommand> orders = new SortedSet<OrderedCommand>();
            List<OrderedCommand> orders = new List<OrderedCommand>();

            Random rnd = new Random();
            for (int i = 1; i <= 100; i++)
            {
                orders.Add(new OrderedCommand()
                {
                    Position = i,
                    OccurAt = TimeSpan.FromMilliseconds( 500 + i * 100 + rnd.Next(500) - 500/2),
                    Message = $"CMD-{i:#00000}"
                });
            }
            //orders.Sort((a, b) => a.OccurAt.CompareTo(b.OccurAt));

            TimeSpan start = TimeSpan.Zero;
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


    public class OrderedCommand// : IComparable<OrderedCommand>
    {
        public int Position = 0;
        public TimeSpan OccurAt = TimeSpan.Zero;
        public string Message;

        //public int CompareTo([AllowNull] OrderedCommand other)
        //{
        //    return this.Position.CompareTo(other.Position);
        //}
    }


    public abstract class ReOrderBufferBase
    {
        protected int _buffer_size_limit = 0;
        protected int _current_index = 0;


        protected ReOrderBufferBase(int bufferSize)
        {
            this._buffer_size_limit = bufferSize;
            this._current_index = 0;
        }

        public abstract bool Push(int data);

        public bool Pop(int data)
        {
            //this._current_index = (data + 1);
            Console.WriteLine($"POP:  {data}");
            return true;
        }
        public bool Skip(int data)
        {
            Console.WriteLine($"SKIP: {data}");
            return true;
        }
    }


    public class DemoReOrderBuffer : ReOrderBufferBase
    {
        private SortedSet<int> _buffer = new SortedSet<int>();

        public DemoReOrderBuffer(int bufferSize) : base(bufferSize)
        {

        }

        public override bool Push(int data)
        {
            if (data < this._current_index)
            {
                // drop
                return false;
            }
            else if (data == this._current_index)
            {
                this.Pop(this._current_index++);
                while (this._buffer.Min == this._current_index)
                {
                    this._buffer.Remove(this._current_index);
                    this.Pop(this._current_index++);
                }

                // return current & follow sequence
                return true;
            }
            else if (this._buffer.Count > this._buffer_size_limit)
            {
                for (; this._current_index < this._buffer.Min; this._current_index++)
                {
                    this.Skip(this._current_index);
                }
                while (this._buffer.Min == this._current_index)
                {
                    this._buffer.Remove(this._current_index);
                    this.Pop(this._current_index++);
                }

                //this.Skip(data);
                //while (this._buffer.Min == this._current_index)
                //{
                //    this._buffer.Remove(this._current_index);
                //    this.Pop(this._current_index++);
                //}

                // out of buffer size, skip 
                return true;
            }
            else
            {
                this._buffer.Add(data);
                return true;
            }
        }
    }

}
