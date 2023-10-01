using System;
using System.Collections.Generic;
using System.Text;

namespace Andrew.ReOrderDemo
{
    public class OrderedCommand
    {
        public int Position = 0;
        public DateTime Origin = DateTime.MinValue;
        public DateTime OccurAt = DateTime.MinValue;
        public string Message;

        public override string ToString()
        {
            return $"{this.Position:#000}, {this.Message}; {this.Origin:HH:mm:ss.fff} -> {this.OccurAt:HH:mm:ss.fff} (Delay: {(this.OccurAt - this.Origin).TotalMilliseconds} msec)";
        }
    }
}
