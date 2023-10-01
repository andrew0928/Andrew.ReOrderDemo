using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;

namespace Andrew.ReOrderDemo
{
    //public interface IReOrderBuffer
    //{

    //    public event ReOrderBufferBase.CommandProcessEventHandler PopCommand;
    //    public event ReOrderBufferBase.CommandProcessEventHandler DropCommand;

    //}

    //public class Demo2 : IReOrderBuffer
    //{
    //    public event ReOrderBufferBase.CommandProcessEventHandler PopCommand;
    //    public event ReOrderBufferBase.CommandProcessEventHandler DropCommand;

    //    public Demo2()
    //    {
    //        this.PopCommand?.Invoke(null, null);
    //    }
    //}

    /*
    public interface IReOrderBufferBase
    {

        public event CommandProcessEventHandler PopCommand;
        public event CommandProcessEventHandler DropCommand;

        public bool Push(OrderedCommand data);
        public bool Flush();

        protected bool Pop(OrderedCommand data, CommandProcessReasonEnum reason);

        protected bool Drop(OrderedCommand data, CommandProcessReasonEnum reason);


        // inner class


        public class CommandProcessEventArgs : EventArgs
        {
            // receive-time
            // process-time

            // process-result: POP | DROP
            // process-reason: PASSTHROU | DELAY | TIMEOUT | BUFFER-FULL | WRONG-ORDER
            public CommandProcessResultEnum Result;
            public CommandProcessReasonEnum Reason;
            public string ReasonMessage;
        }

        public delegate void CommandProcessEventHandler(OrderedCommand sender, CommandProcessEventArgs args);

        public enum CommandProcessResultEnum
        {
            POP,
            DROP,
        }

        public enum CommandProcessReasonEnum
        {
            POP_PASSTHRU,
            POP_BUFFERED,

            //POP_TIMEOUT,
            DROP_BUFFER_SIZE_FULL,
            DROP_BUFFER_DURATION_FULL,

            DROP_WRONG_ORDER,
            DROP_FORCE_FLUSH,
        }

        public class OrderedCommandComparer : IComparer<OrderedCommand>
        {
            public int Compare([AllowNull] OrderedCommand x, [AllowNull] OrderedCommand y)
            {
                return x.Position.CompareTo(y.Position);
            }
        }
    }
    */

    
    public abstract class ReOrderBufferBase
    {

        

        public class CommandProcessEventArgs : EventArgs
        {
            // receive-time
            // process-time

            // process-result: POP | DROP
            // process-reason: PASSTHROU | DELAY | TIMEOUT | BUFFER-FULL | WRONG-ORDER
            public CommandProcessResultEnum Result;
            public CommandProcessReasonEnum Reason;
            public string ReasonMessage;
        }

        public delegate void CommandProcessEventHandler(OrderedCommand sender, CommandProcessEventArgs args);

        public enum CommandProcessResultEnum
        {
            POP,
            DROP,
        }

        public enum CommandProcessReasonEnum
        {
            POP_PASSTHRU,
            POP_BUFFERED,

            //POP_TIMEOUT,
            DROP_BUFFER_SIZE_FULL,
            DROP_BUFFER_DURATION_FULL,

            DROP_WRONG_ORDER,
            DROP_FORCE_FLUSH,
        }

        public event CommandProcessEventHandler PopCommand;
        public event CommandProcessEventHandler DropCommand;


        protected ReOrderBufferBase()
        {
        }


        public abstract bool Push(OrderedCommand data);
        //{
        //    this._metrics_total_push++;

        //    //int current_buffer_size = this._metrics_total_push - this._metrics_total_pop - this._metrics_total_drop;
        //    //this._metrics_buffer_max = Math.Max(current_buffer_size, this._metrics_buffer_max);

        //    //Console.WriteLine($"PUSH: {data}");
        //    return this._Push(data);
        //}

        public abstract bool Flush();
        //{
        //    return this._Flush();
        //}

        //protected abstract bool _Push(OrderedCommand data);
        //protected abstract bool _Flush();

        protected bool Pop(OrderedCommand data, CommandProcessReasonEnum reason)
        {
            //this._metrics_total_pop++;
            //Console.WriteLine($"POP:  {data.Position:#000}, {data.Message};");

            this.PopCommand?.Invoke(data, new CommandProcessEventArgs()
            {
                Result = CommandProcessResultEnum.POP,
                Reason = reason,
            });

            return true;
        }

        


        protected bool Drop(OrderedCommand data, CommandProcessReasonEnum reason)
        {
            //this._metrics_total_drop++;
            //Console.WriteLine($"DROP: {data.Position:#000}, {data.Message}; ({reason})");

            this.DropCommand?.Invoke(data, new CommandProcessEventArgs()
            {
                Result = CommandProcessResultEnum.DROP,
                Reason = reason,
            });

            return true;
        }


        //public void DumpMetrics(TextWriter writer)
        //{
        //    writer.WriteLine();
        //    writer.WriteLine($"ReOrder Metrics:");
        //    writer.WriteLine($"- total push: {this._metrics_total_push}");
        //    writer.WriteLine($"- total pop:  {this._metrics_total_pop}");
        //    writer.WriteLine($"- total drop: {this._metrics_total_drop}");
        //    writer.WriteLine($"- drop rate: {this._metrics_total_drop * 100D / this._metrics_total_push:#.##} %");
        //    writer.WriteLine($"- max buffer size: {this._metrics_buffer_max}");
        //}


        public class OrderedCommandComparer : IComparer<OrderedCommand>
        {
            public int Compare([AllowNull] OrderedCommand x, [AllowNull] OrderedCommand y)
            {
                return x.Position.CompareTo(y.Position);
            }
        }
    }
    
}
