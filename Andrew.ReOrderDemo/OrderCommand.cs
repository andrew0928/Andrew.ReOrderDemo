using System;
using System.Collections.Generic;
using System.Text;

namespace Andrew.ReOrderDemo
{
    public class OrderedCommand
    {
        // 流水號，從 0 開始，按照約定必須是連續編號
        public int Position = 0;

        // command 建立的時間
        public DateTime Origin = DateTime.MinValue;

        // command 收到時間
        public DateTime OccurAt = DateTime.MinValue;

        // command 內容說明
        public string Message;
    }
}
