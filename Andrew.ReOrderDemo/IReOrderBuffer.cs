using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;

namespace Andrew.ReOrderDemo
{
    public interface IReOrderBuffer
    {
        public bool Push(OrderedCommand data);
        public bool Flush();

        public event CommandProcessEventHandler CommandIsReadyToSend;
        public event CommandProcessEventHandler CommandWasDroped;
    }



    public class CommandProcessEventArgs : EventArgs
    {
        public CommandProcessResultEnum Result;
        public CommandProcessReasonEnum Reason;
        public string Message;
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
