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
            //Demo1_ExecuteCommandWithoutReordering(args);
            Demo2_ExecuteCommandWithReorderBuffer(args);
        }


        static void Demo1_ExecuteCommandWithoutReordering(string[] args)
        {
            int command_period = 100;
            int command_noise = 500;

            if (args.Length != 2)
            {
                Console.WriteLine($"Usage: [execute] {{command period in msec}} {{command noise}}");
                Console.WriteLine($"- no arguments, use default value ({command_period} msec, {command_noise}) instead.");
            }
            else
            {
                command_period = int.Parse(args[0]);
                command_noise = int.Parse(args[1]);
            }

            foreach (var x in GetCommands(command_period, command_noise))
            {
                ExecuteCommand(x);
            }
            return;
        }

        static void Demo2_ExecuteCommandWithReorderBuffer(string[] args)
        {
            int command_period = 100;
            int command_noise = 500;
            int duration_msec = 100;
            int buffer_size = 10;

            if (args.Length != 2)
            {
                Console.WriteLine($"Usage: [execute] {{command period in msec}} {{command noise}} {{buffer duration in msec}} {{buffer size}}");
                Console.WriteLine($"- no arguments, use default value ({command_period} msec, {command_noise}, {duration_msec} msec, {buffer_size}) instead.");
            }
            else
            {
                command_period = int.Parse(args[0]);
                command_noise = int.Parse(args[1]);
                duration_msec = int.Parse(args[2]);
                buffer_size = int.Parse(args[3]);
            }

            DateTimeUtil.Init(new DateTime(2023, 09, 16));
            IReOrderBuffer ro = new ReOrderBuffer(TimeSpan.FromMilliseconds(duration_msec), buffer_size);


            int _log_sequence = 0;
            Console.Error.WriteLine($"TimeInSec,Push,Pop,Drop,BufferMax,AverageLatency");

            var overall_metrics = (ro as ReOrderBuffer).ResetMetrics();
            DateTimeUtil.Instance.RaiseSecondPassEvent += (sender, args) =>
            {
                // write metrics
                Interlocked.Increment(ref _log_sequence);
                var metrics = (ro as ReOrderBuffer).ResetMetrics();
                Console.Error.WriteLine($"{_log_sequence},{metrics.push},{metrics.pop},{metrics.drop},{metrics.buffer_max},{metrics.latency / metrics.pop}");

                // update overall statistics
                overall_metrics.push += metrics.push;
                overall_metrics.pop += metrics.pop;
                overall_metrics.drop += metrics.drop;
                overall_metrics.buffer_max = Math.Max(metrics.buffer_max, overall_metrics.buffer_max);
                overall_metrics.latency += metrics.latency; //(overall_metrics.latency * overall_metrics.pop + metrics.latency * metrics.pop) / (overall_metrics.pop + metrics.pop);
            };




            //ReOrderBufferBase.CommandProcessEventHandler dump = 

            ro.CommandIsReadyToSend += (sender, args) =>
            {
                // Console.WriteLine($"- {args.Reason,-20},  #{sender.Position}, {(sender.OccurAt - sender.Origin).TotalMilliseconds,5} msec, {sender.Message}");
                ExecuteCommand(sender);
            };

            ro.CommandWasDroped += (sender, args) =>
            {
                Console.WriteLine($"- {args.Reason,-20},  #{sender.Position}, {(sender.OccurAt - sender.Origin).TotalMilliseconds,5} msec, {sender.Message}");
            };


            foreach (var item in GetCommands(command_period, command_noise))
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
            Console.WriteLine($"- Buffer Delay:  {overall_metrics.latency / overall_metrics.pop} msec");
            Console.WriteLine($"- Buffer Usage:  {overall_metrics.buffer_max}");
        }



        /// <summary>
        /// Source - GetCommands()
        /// </summary>
        /// <param name="boost"></param>
        /// <returns></returns>
        static IEnumerable<OrderedCommand> GetCommands(int period = 100, int noise = 500)
        {
            int total_count = 1000;
            TimeSpan cmd_period = TimeSpan.FromMilliseconds(period);
            int cmd_noise = noise;


            List<OrderedCommand> orders = new List<OrderedCommand>();
            DateTime start = //DateTime.Now;
                DateTimeUtil.Instance.Now.AddSeconds(1.0); // warn up time

            Random rnd = new Random(867);


            //Console.WriteLine($"Position,OriginDateTime,OccurAtDateTime");
            for (int i = 0; i < total_count; i++)
            {
                if (rnd.Next(100) == 0) continue;   // 1% lost rate
                var order = new OrderedCommand()
                {
                    Position = i,
                    Origin = start + cmd_period * i, // * ((double)total_count + i) / total_count, // TimeSpan.FromMilliseconds(i * cmd_period),
                    OccurAt = start + cmd_period * i + TimeSpan.FromMilliseconds(rnd.Next(cmd_noise)), //TimeSpan.FromMilliseconds(i * cmd_period + rnd.Next(cmd_noise)),
                    Message = $"CMD-{i:#00000}"
                };
                orders.Add(order);
                //Console.WriteLine($"{order.Position},{(order.Origin - start).TotalMilliseconds},{(order.OccurAt - start).TotalMilliseconds}");
            }
            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine();

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















































