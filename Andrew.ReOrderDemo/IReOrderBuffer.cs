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
        public event CommandSkipEventHandler CommandWasSkipped;
    }



    public class CommandProcessEventArgs : EventArgs
    {
        public CommandProcessResultEnum Result;
        public CommandProcessReasonEnum Reason;
        public string Message;
    }

    public delegate void CommandProcessEventHandler(OrderedCommand sender, CommandProcessEventArgs args);
    public delegate void CommandSkipEventHandler(int position, CommandProcessEventArgs args);

    public enum CommandProcessResultEnum
    {
        SEND,
        DROP,
        SKIP
    }

    public enum CommandProcessReasonEnum
    {
        // 收到直接送出
        SEND_PASSTHRU,

        // 從 Buffer 內送出
        SEND_BUFFERED,

        // 因為 Buffer 已滿, 被迫丟棄
        DROP_BUFFERFULL,

        // 因為非預期的順序 (判定是已不處理的範圍, 直接丟棄)
        DROP_OUTOFORDER,

        // 因為 Buffer 已滿, 被迫略過等待還未收到的中間訊息
        SKIP_BUFFERFULL,
    }

    public class OrderedCommandComparer : IComparer<OrderedCommand>
    {
        public int Compare([AllowNull] OrderedCommand x, [AllowNull] OrderedCommand y)
        {
            return x.Position.CompareTo(y.Position);
        }
    }
}
