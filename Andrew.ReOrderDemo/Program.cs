using System;
using System.Collections.Generic;

namespace Andrew.ReOrderDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var ro = new ReOrderBuffer(3);

            foreach(var item in GetData())
            {
                ro.Push(item);
            }
        }

        static IEnumerable<int> GetData()
        {
            return new int[] { 3, 2, 1, 0, 4, 5, 9, 6, 7, 10, 8, 11, 12, 20, 19, 13, 14, 18, 17, 16, 15,30,29,28,27,26,25,24,23,22,21 };
        }
    }



    public class ReOrderBuffer
    {
        private int _buffer_size_limit = 0;
        private int _current_index = 0;

        private SortedSet<int> _buffer = new SortedSet<int>();

        public ReOrderBuffer(int bufferSize)
        {
            this._buffer_size_limit = bufferSize;
            this._current_index = 0;
        }

        public bool Push(int data)
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
}
