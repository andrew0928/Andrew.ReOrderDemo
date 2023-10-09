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
            
            int buffer_size = 10;

            if (args.Length == 0)
            {
                Console.WriteLine($"Usage: [execute] {{command period in msec}} {{command noise}} {{buffer size}}");
                return;
            }
            else
            {
                command_period = int.Parse(args[0]);
                command_noise = int.Parse(args[1]);
                buffer_size = int.Parse(args[2]);
            }

            DateTimeUtil.Init(new DateTime(2023, 09, 16));
            IReOrderBuffer ro = new ReOrderBuffer(buffer_size);


            int _log_sequence = 0;
            Console.Error.WriteLine($"TimeInSec,Push,Send,Drop,Skip,BufferMax,Delay");

            var overall_metrics = (ro as ReOrderBuffer).ResetMetrics();
            DateTimeUtil.Instance.RaiseSecondPassEvent += (sender, args) =>
            {
                // write metrics
                Interlocked.Increment(ref _log_sequence);
                var metrics = (ro as ReOrderBuffer).ResetMetrics();
                double avg_latency = 0;
                if (metrics.send > 0) avg_latency = metrics.delay / metrics.send;
                Console.Error.WriteLine($"{_log_sequence},{metrics.push},{metrics.send},{metrics.drop},{metrics.skip},{metrics.buffer_max},{avg_latency}");

                // update overall statistics
                overall_metrics.push += metrics.push;
                overall_metrics.send += metrics.send;
                overall_metrics.drop += metrics.drop;
                overall_metrics.skip += metrics.skip;
                overall_metrics.buffer_max = Math.Max(metrics.buffer_max, overall_metrics.buffer_max);
                overall_metrics.delay += metrics.delay;
            };


            ro.CommandIsReadyToSend += (sender, args) =>
            {
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


            DateTimeUtil.Instance.TimePass(TimeSpan.FromSeconds(10));

            Console.WriteLine($"ReOrderBuffer Overall Metrics:");
            Console.WriteLine($"- Push:          {overall_metrics.push}");
            Console.WriteLine($"- Send:          {overall_metrics.send}");
            Console.WriteLine($"- Drop:          {overall_metrics.drop}");
            Console.WriteLine($"- Drop Rate (%)  {overall_metrics.drop * 100 / overall_metrics.push} %");
            Console.WriteLine($"- Command Delay: {overall_metrics.delay / overall_metrics.send:0.000} msec");
            Console.WriteLine($"- Buffer Usage:  {overall_metrics.buffer_max}");
        }


        // 模擬實際接收到的 Command 順序 (會按照亂數前後位移)
        static IEnumerable<OrderedCommand> GetCommands(int period = 100, int noise = 500)
        {
            int total_count = 1000;
            TimeSpan cmd_period = TimeSpan.FromMilliseconds(period);
            int cmd_noise = noise;

            List<OrderedCommand> orders = new List<OrderedCommand>();
            DateTime start = DateTimeUtil.Instance.Now.AddSeconds(1.0); // warn up time

            Random rnd = new Random(867);

            for (int i = 0; i < total_count; i++)
            {
                //if (rnd.Next(100) == 0)
                //{
                //    Console.WriteLine($"RANDOM-LOST: {i}");
                //    continue;   // 1% lost rate
                //}

                //
                // todo: 隨機可以改成高斯分布
                //
                var order = new OrderedCommand()
                {
                    Position = i,
                    Origin = start + cmd_period * i,
                    OccurAt = start + cmd_period * i + TimeSpan.FromMilliseconds(rnd.Next(cmd_noise)), 
                    Message = $"CMD-{i:#00000}"
                };
                orders.Add(order);
            }

            int check_count = 0;
            foreach (var c in (from x in orders orderby x.OccurAt ascending select x))
            {
                DateTimeUtil.Instance.TimeSeek(c.OccurAt);
                check_count++;
                yield return c;
            }

            Console.WriteLine($"CHECK-COUNT: {check_count}, {orders.Count}");
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















































