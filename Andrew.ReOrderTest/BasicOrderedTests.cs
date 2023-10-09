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
        public void BasicScenario5_MissCommand()
        {
            this.SequenceTest(
                100,
                new int[] { 0, 1, 2, 3, 4,    6, 7, 8, 9, 10 },
                new int[] { 0, 1, 2, 3, 4 });
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








        [TestInitialize]
        public void Setup()
        {
            DateTimeUtil.Reset();
            DateTimeUtil.Init(new DateTime(2023, 09, 16));
        }

        private IEnumerable<OrderedCommand> GetBasicCommands(params int[] sequences)
        {
            //DateTime start = DateTimeUtil.Instance.Now.AddMilliseconds(5000);


            //int count = 0;
            foreach (int index in sequences)
            {
                //DateTime _temp_origin = start.AddMilliseconds(index * 100);
                //DateTime _temp_occurat = start.AddMilliseconds(sequences.Length * 100 + count * 100);

                //DateTimeUtil.Instance.TimeSeek(_temp_occurat);

                yield return new OrderedCommand()
                {
                    Position = index,
                    Origin = DateTimeUtil.Instance.Now,
                    OccurAt = DateTimeUtil.Instance.Now,
                };
            }
        }

        private void SequenceTest(int buffer_size, int[] source_sequence, int[] expect_sequence)
        {
            IReOrderBuffer buffer = new ReOrderBuffer(buffer_size);

            int count = 0;
            buffer.CommandIsReadyToSend += (sender, args) =>
            {
                Console.WriteLine(sender.Position);
                Assert.AreEqual(expect_sequence[count], sender.Position);
                count++;
            };

            foreach (var cmd in this.GetBasicCommands(source_sequence))
            {
                bool result = buffer.Push(cmd);
            }

            buffer.Flush();
            Assert.AreEqual(expect_sequence.Length, count);
        }

    }
}
