using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Andrew.ReOrderDemo
{
    class Program
    {

        static void Main(string[] args)
        {
            //Demo1_ExecuteCommandWithoutReordering();
            Demo2_ExecuteCommandWithReorderBuffer(args);
        }


        static void Demo1_ExecuteCommandWithoutReordering()
        {
            foreach (var x in GetCommands(true))
            {
                ExecuteCommand(x);
            }
            return;
        }

        static void Demo2_ExecuteCommandWithReorderBuffer(string[] args)
        {
            int duration_msec = 100;
            int buffer_size = 10;

            if (args.Length != 2)
            {
                Console.WriteLine($"Usage: [execute] {{buffer duration in msec}} {{buffer size}}");
                Console.WriteLine($"- no arguments, use default value ({duration_msec} msec, {buffer_size}) instead.");
            }
            else
            {
                duration_msec = int.Parse(args[0]);
                buffer_size = int.Parse(args[1]);
            }

            DateTimeUtil.Init(new DateTime(2023, 09, 16));
            IReOrderBufferBase ro = new DemoReOrderBuffer(TimeSpan.FromMilliseconds(duration_msec), buffer_size);


            int _log_sequence = 0;
            Console.Error.WriteLine($"TimeInSec,Push,Pop,Drop,BufferMax");

            var overall_metrics = (ro as DemoReOrderBuffer).ResetMetrics();
            DateTimeUtil.Instance.RaiseSecondPassEvent += (sender, args) =>
            {
                // write metrics
                Interlocked.Increment(ref _log_sequence);
                var metrics = (ro as DemoReOrderBuffer).ResetMetrics();
                Console.Error.WriteLine($"{_log_sequence},{metrics.push},{metrics.pop},{metrics.drop},{metrics.buffer_max}");

                // update overall statistics
                overall_metrics.push += metrics.push;
                overall_metrics.pop += metrics.pop;
                overall_metrics.drop += metrics.drop;
                overall_metrics.buffer_max = Math.Max(metrics.buffer_max, overall_metrics.buffer_max);

            };




            //ReOrderBufferBase.CommandProcessEventHandler dump = 

            ro.PopCommand += (sender, args) =>
            {
                // Console.WriteLine($"- {args.Reason,-20},  #{sender.Position}, {(sender.OccurAt - sender.Origin).TotalMilliseconds,5} msec, {sender.Message}");
                ExecuteCommand(sender);
            };

            ro.DropCommand += (sender, args) =>
            {
                Console.WriteLine($"- {args.Reason,-20},  #{sender.Position}, {(sender.OccurAt - sender.Origin).TotalMilliseconds,5} msec, {sender.Message}");
            };


            foreach (var item in GetCommands(true))
            {
                ro.Push(item);
            }
            ro.Flush();

            DateTimeUtil.Instance.TimePass(TimeSpan.FromSeconds(5));

            //ro.DumpMetrics(Console.Out);

            Console.WriteLine($"ReOrderBuffer Overall Metrics:");
            Console.WriteLine($"- Push:          {overall_metrics.push}");
            Console.WriteLine($"- Pop:           {overall_metrics.pop}");
            Console.WriteLine($"- Drop:          {overall_metrics.drop}");
            Console.WriteLine($"- Drop Rate (%)  {overall_metrics.drop * 100 / overall_metrics.push} %");
            Console.WriteLine($"- Buffer Usage:  {overall_metrics.buffer_max}");
        }



        /// <summary>
        /// Source - GetCommands()
        /// </summary>
        /// <param name="boost"></param>
        /// <returns></returns>
        static IEnumerable<OrderedCommand> GetCommands(bool boost = true)
        {
            int total_count = 1000;
            TimeSpan cmd_period = TimeSpan.FromMilliseconds(30);
            int cmd_noise = 500;


            List<OrderedCommand> orders = new List<OrderedCommand>();
            DateTime start = //DateTime.Now;
                DateTimeUtil.Instance.Now.AddSeconds(1.0); // warn up time

            Random rnd = new Random(867);


            Console.WriteLine($"Position,OriginDateTime,OccurAtDateTime");
            for (int i = 0; i < total_count; i++)
            {
                var order = new OrderedCommand()
                {
                    Position = i,
                    Origin = start + cmd_period * i, // TimeSpan.FromMilliseconds(i * cmd_period),
                    OccurAt = start + cmd_period * i + TimeSpan.FromMilliseconds(rnd.Next(cmd_noise)), //TimeSpan.FromMilliseconds(i * cmd_period + rnd.Next(cmd_noise)),
                    Message = $"CMD-{i:#00000}"
                };
                orders.Add(order);
                Console.WriteLine($"{order.Position},{(order.Origin - start).TotalMilliseconds},{(order.OccurAt - start).TotalMilliseconds}");
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            foreach (var c in (from x in orders orderby x.OccurAt ascending select x))
            {
                //if (!boost)
                //{
                //    Task.Delay(c.OccurAt - start).Wait();
                //    start = c.OccurAt;
                //}
                //Console.WriteLine($"----- {c.Position}, {(c.OccurAt - start).TotalMilliseconds} ({c.OccurAt.Millisecond})");
                DateTimeUtil.Instance.TimeSeek(c.OccurAt);
                yield return c;
            }
        }


        static object _sync_command = new object();
        static int _last_command_position = 0;
        static bool ExecuteCommand(OrderedCommand cmd)
        {
            if (cmd == null) return false;
            if (cmd.Position <= _last_command_position)
            {
                Console.WriteLine("Execute Command Fail: Wrong Orders...");
                return false;
            }

            lock(_sync_command)
            {
                if (cmd.Position <= _last_command_position)
                {
                    Console.WriteLine("Execute Command Fail: Wrong Orders...");
                    return false;
                }
                _last_command_position = cmd.Position;
            }

            Console.WriteLine($"Execute Command: {cmd}");
            return true;
        }
    }










}















































