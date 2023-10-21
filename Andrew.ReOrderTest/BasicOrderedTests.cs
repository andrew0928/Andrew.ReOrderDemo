using Andrew.ReOrderDemo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Andrew.ReOrderTest
{
    [TestClass]
    public class BasicOrderedTests
    {
        [TestMethod]
        public void BasicScenario1_PerfectOrdered()
        {
            this.SequenceTest(
                100,
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario2_OutOfOrderCommand()
        {
            this.SequenceTest(
                100,
                new int[] { 0, 1, 2, 3, 5, 4, 6, 7, 8, 9, 10 },
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario3_OutOfOrderCommand()
        {
            this.SequenceTest(
                100,
                new int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 },
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario4_OutOfOrderCommand()
        {
            this.SequenceTest(
                100,
                new int[] { 0, 3, 2, 1, 6, 5, 4, 9, 8, 7, 10 },
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario5_LostCommand()
        {
            this.SequenceTest(
                100,
                new int[] { 0, 1, 2, 3, 4,    6, 7, 8, 9, 10 },
                new int[] { 0, 1, 2, 3, 4,    6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario6_BufferLimit()
        {
            this.SequenceTest(
                3,
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario7_BufferLimit()
        {
            this.SequenceTest(
                3,
                new int[] { 0, 1, 2, 4, 3, 5, 6, 7, 8, 9, 10 }, // offset 1
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario8_BufferLimit()
        {
            this.SequenceTest(
                3,
                new int[] { 0, 1, 2, 5, 4, 3, 6, 7, 8, 9, 10 }, // offset 2
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario9_BufferLimit()
        {
            this.SequenceTest(
                3,
                new int[] { 0, 1, 2, 6, 5, 4, 3, 7, 8, 9, 10 }, // offset 3
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario10_BufferLimit()
        {
            this.SequenceTest(
                3,
                new int[] { 0, 1, 2, 7, 6, 5, 4, 3, 8, 9, 10 }, // offset 4
                new int[] { 0, 1, 2,    4, 5, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario11_BufferLimit()
        {
            this.SequenceTest(
                3,
                new int[] { 0, 1, 2, 8, 7, 6, 5, 4, 3, 9, 10 }, // offset 5
                new int[] { 0, 1, 2, 5, 6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario12_BufferLimit()
        {
            this.SequenceTest(
                3,
                new int[] { 0, 5, 4, 3, 2, 1, 10, 9, 8, 7, 6 }, // offset 4 x 2
                new int[] { 0,    2, 3, 4, 5,     7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario13_BufferLimitAndLostCommand()
        {
            this.SequenceTest(
                3,
                new int[] { 0, 1, 2, 3, 4,    6, 7, 8, 9, 10 },
                new int[] { 0, 1, 2, 3, 4,    6, 7, 8, 9, 10 });
        }

        [TestMethod]
        public void BasicScenario14_ArticleDemo1()
        {
            this.SequenceTest(
                100,
                new int[] { 0, 1, 3, 4, 2 },
                new int[] { 0, 1, 2, 3, 4 });
        }
        [TestMethod]
        public void BasicScenario15_ArticleDemo2()
        {
            this.SequenceTest(
                100,
                new int[] { 0, 1, 3, 5, 2, 6, 4, 7, 8 },
                new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });
        }






        [TestInitialize]
        public void Setup()
        {
            DateTimeUtil.Reset();
            DateTimeUtil.Init(new DateTime(2023, 09, 16));
        }

        private IEnumerable<OrderedCommand> GetBasicCommands(params int[] sequences)
        {
            DateTime start = DateTimeUtil.Instance.Now.AddMilliseconds(5000);


            int count = 0;
            foreach (int index in sequences)
            {
                count++;
                DateTime _temp_origin = start.AddMilliseconds(count * 100);
                DateTime _temp_occurat = start.AddMilliseconds(count * 100 + 100);

                DateTimeUtil.Instance.TimeSeek(_temp_occurat);

                yield return new OrderedCommand()
                {
                    Position = index,
                    Origin = _temp_origin, //DateTimeUtil.Instance.Now,
                    OccurAt = _temp_occurat, //DateTimeUtil.Instance.Now,
                };
            }
        }

        private void SequenceTest(int buffer_size, int[] source_sequence, int[] expect_sequence)
        {
            IReOrderBuffer buffer = new ReOrderBuffer(buffer_size);

            int count = 0;
            buffer.CommandIsReadyToSend += (sender, args) =>
            {
                Console.WriteLine($"SEND: {sender.Position} - {args.Reason}");
                Assert.AreEqual(expect_sequence[count], sender.Position);
                count++;
            };
            buffer.CommandWasDroped += (sender, args) =>
            {
                Console.WriteLine($"DROP: {sender.Position} - {args.Reason}");
            };
            buffer.CommandWasSkipped += (sender, args) =>
            {
                Console.WriteLine($"SKIP: {sender} - {args.Reason}");
            };

            foreach (var cmd in this.GetBasicCommands(source_sequence))
            {
                Console.WriteLine($"PUSH: {cmd.Position}");
                bool result = buffer.Push(cmd);
                Console.WriteLine($"      (Buffer: {(buffer as ReOrderBuffer).DumpBuffer()})");
            }

            buffer.Flush();
            Assert.AreEqual(expect_sequence.Length, count);


            var metrics = (buffer as ReOrderBuffer).ResetMetrics();
            Console.WriteLine($"-----------------------------------");
            Console.WriteLine($"Metrics:");
            Console.WriteLine($"- PUSH:          {metrics.push}");
            Console.WriteLine($"- SEND:          {metrics.send}");
            Console.WriteLine($"- DROP:          {metrics.drop}");
            Console.WriteLine($"- SKIP:          {metrics.skip}");

            //Console.WriteLine($"- Command Delay: {metrics.command_delay / metrics.push:0.000} msec");
            Console.WriteLine($"- Max Delay:     {metrics.max_delay:0.000} msec");
            Console.WriteLine($"- Average Delay: {metrics.total_delay / metrics.push:0.000} msec");
            Console.WriteLine($"- Buffer Usage:  {metrics.buffer_max}");
        }

    }
}
