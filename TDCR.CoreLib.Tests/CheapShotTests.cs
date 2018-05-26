using NUnit.Framework;
using System;
using System.Collections.Generic;
using TDCR.CoreLib.HistoryCollection;
using System.Text;
using TDCR.CoreLib.Messages.Raft;
using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Tests
{
    [TestFixture]
    class CheapShotTests
    {
        readonly static Entry exA = new Entry { Event = new Uid(), Tag = new CommandTag { Type = CommandTag.CommandType.Exec, Uid = new Uid() } };
        readonly static Entry exA1 = new Entry { Event = exA.Event, Tag = new CommandTag { Type = CommandTag.CommandType.Exec, Uid = new Uid() } };
        readonly static Entry exA2 = new Entry { Event = exA.Event, Tag = new CommandTag { Type = CommandTag.CommandType.Exec, Uid = new Uid() } };
        readonly static Entry exB = new Entry { Event = new Uid(), Tag = new CommandTag { Type = CommandTag.CommandType.Exec, Uid = new Uid() } };
        readonly static Entry exC = new Entry { Event = new Uid(), Tag = new CommandTag { Type = CommandTag.CommandType.Exec, Uid = new Uid() } };
        readonly static Entry exD = new Entry { Event = new Uid(), Tag = new CommandTag { Type = CommandTag.CommandType.Exec, Uid = new Uid() } };

        readonly static EventExecution eea = new EventExecution(exA.Event, exA.Tag.Uid);
        readonly static EventExecution eea1 = new EventExecution(exA.Event, exA1.Tag.Uid);
        readonly static EventExecution eea2 = new EventExecution(exA.Event, exA2.Tag.Uid);
        readonly static EventExecution eeb = new EventExecution(exB.Event, exB.Tag.Uid);
        readonly static EventExecution eec = new EventExecution(exC.Event, exC.Tag.Uid);
        readonly static EventExecution eed = new EventExecution(exD.Event, exD.Tag.Uid);

        readonly CheapShot AllInvalid = new CheapShot(new List<Tuple<Uid, Entry[]>>
            {
                new Tuple<Uid, Entry[]>(exA.Event, new Entry[] { exB, exA }), // A
                new Tuple<Uid, Entry[]>(exB.Event, new Entry[] { exC, exB }), // B
                new Tuple<Uid, Entry[]>(exC.Event, new Entry[] { exD, exC }), // C
                new Tuple<Uid, Entry[]>(exD.Event, new Entry[] { }) // D
            });

        readonly CheapShot AllValid = new CheapShot(new List<Tuple<Uid, Entry[]>>
            {
                new Tuple<Uid, Entry[]>(exA.Event, new Entry[] { exB, exA }), // A
                new Tuple<Uid, Entry[]>(exB.Event, new Entry[] { exC, exB }), // B
                new Tuple<Uid, Entry[]>(exC.Event, new Entry[] { exD, exC }), // C
                new Tuple<Uid, Entry[]>(exD.Event, new Entry[] { exD }) // D
            });

        readonly CheapShot PartiallyValid = new CheapShot(new List<Tuple<Uid, Entry[]>>
            {
                new Tuple<Uid, Entry[]>(exA.Event, new Entry[] { exA, exB }), // A
                new Tuple<Uid, Entry[]>(exB.Event, new Entry[] { exB, exC }), // B
                new Tuple<Uid, Entry[]>(exC.Event, new Entry[] { exD, exC }), // C
                new Tuple<Uid, Entry[]>(exD.Event, new Entry[] { }) // D
            });

        readonly CheapShot SingleEventMultipleExecutions = new CheapShot(new List<Tuple<Uid, Entry[]>>
            {
                new Tuple<Uid, Entry[]>(exA.Event, new Entry[] { exA, exA1, exA2 })
            });

        [Test]
        public void AllInvalidConsistentCutTest()
        {
            AllInvalid.CarveConsistentCut();

            Assert.That(AllInvalid.Observed[0].Item2[0].Valid == false);
            Assert.That(AllInvalid.Observed[0].Item2[1].Valid == false);
            Assert.That(AllInvalid.Observed[1].Item2[0].Valid == false);
            Assert.That(AllInvalid.Observed[1].Item2[1].Valid == false);
            Assert.That(AllInvalid.Observed[2].Item2[0].Valid == false);
            Assert.That(AllInvalid.Observed[2].Item2[1].Valid == false);
        }

        [Test]
        public void AllValidConsistentCutTest()
        {
            AllValid.CarveConsistentCut();

            Assert.That(AllValid.Observed[0].Item2[0].Valid == true);
            Assert.That(AllValid.Observed[0].Item2[1].Valid == true);
            Assert.That(AllValid.Observed[1].Item2[0].Valid == true);
            Assert.That(AllValid.Observed[1].Item2[1].Valid == true);
            Assert.That(AllValid.Observed[2].Item2[0].Valid == true);
            Assert.That(AllValid.Observed[2].Item2[1].Valid == true);
            Assert.That(AllValid.Observed[3].Item2[0].Valid == true);
        }

        [Test]
        public void PartiallyValidConsistentCutTest()
        {
            PartiallyValid.CarveConsistentCut();

            Assert.That(PartiallyValid.Observed[0].Item2[0].Valid == true);
            Assert.That(PartiallyValid.Observed[0].Item2[1].Valid == true);
            Assert.That(PartiallyValid.Observed[1].Item2[0].Valid == true);
            Assert.That(PartiallyValid.Observed[1].Item2[1].Valid == false);
            Assert.That(PartiallyValid.Observed[2].Item2[0].Valid == false);
            Assert.That(PartiallyValid.Observed[2].Item2[1].Valid == false);
        }

        [Test]
        public void SingleEventMultipleExecutionsConsistentCutTest()
        {
            SingleEventMultipleExecutions.CarveConsistentCut();

            Assert.That(SingleEventMultipleExecutions.Observed[0].Item2[0].Valid == true);
            Assert.That(SingleEventMultipleExecutions.Observed[0].Item2[1].Valid == true);
            Assert.That(SingleEventMultipleExecutions.Observed[0].Item2[2].Valid == true);
        }

        [Test]
        public void AllInvalidBuildGraphTest()
        {
            AllInvalid.CarveConsistentCut();
            AllInvalid.BuildGraph();
            var graph = AllInvalid.Graph;

            Assert.That(graph.Keys.Count == 0);
        }

        [Test]
        public void AllValidBuildGraphTest()
        {
            AllValid.CarveConsistentCut();
            AllValid.BuildGraph();
            var graph = AllValid.Graph;

            graph.TryGetValue(eea, out List<EventExecution> nodeA);
            graph.TryGetValue(eeb, out List<EventExecution> nodeB);
            graph.TryGetValue(eec, out List<EventExecution> nodeC);
            graph.TryGetValue(eed, out List<EventExecution> nodeD);

            Assert.That(graph.Keys.Count == 4);
            Assert.That(nodeA.Count == 0);
            Assert.That(nodeB.Count == 1);
            Assert.AreEqual(nodeB[0], eea);
            Assert.That(nodeC.Count == 1);
            Assert.AreEqual(nodeC[0], eeb);
            Assert.That(nodeD.Count == 1);
            Assert.AreEqual(nodeD[0], eec);
        }

        [Test]
        public void PartiallyValidBuildGraphTest()
        {
            PartiallyValid.CarveConsistentCut();
            PartiallyValid.BuildGraph();
            var graph = PartiallyValid.Graph;

            graph.TryGetValue(eea, out List<EventExecution> nodeA);
            graph.TryGetValue(eeb, out List<EventExecution> nodeB);
            graph.TryGetValue(eec, out List<EventExecution> nodeC);

            Assert.That(graph.Keys.Count == 2);
            Assert.That(nodeA.Count == 1);
            Assert.AreEqual(nodeA[0], eeb);
            Assert.That(nodeB.Count == 0);
        }

        [Test]
        public void SingleEventMultipleExecutionBuildGraphTest()
        {
            SingleEventMultipleExecutions.CarveConsistentCut();
            SingleEventMultipleExecutions.BuildGraph();
            var graph = SingleEventMultipleExecutions.Graph;

            graph.TryGetValue(eea, out List<EventExecution> nodeA);
            graph.TryGetValue(eea1, out List<EventExecution> nodeA1);
            graph.TryGetValue(eea2, out List<EventExecution> nodeA2);

            Assert.That(graph.Keys.Count == 3);
            Assert.That(nodeA.Count == 1);
            Assert.That(nodeA1.Count == 1);
            Assert.That(nodeA2.Count == 0);
        }

        [Test]
        public void AllInvalidTopOrdering()
        {
            AllInvalid.CarveConsistentCut();
            AllInvalid.BuildGraph();
            AllInvalid.GetCycleTopologicalOrdering();

            Assert.That(AllInvalid.GlobalHistory.Length == 0);
        }

        [Test]
        public void AllValidTopOrdering()
        {
            AllValid.CarveConsistentCut();
            AllValid.BuildGraph();
            AllValid.GetCycleTopologicalOrdering();

            var executionHistory = new EventExecution[] {
                new EventExecution(exD.Event, exD.Tag.Uid),
                new EventExecution(exC.Event, exC.Tag.Uid),
                new EventExecution(exB.Event, exB.Tag.Uid),
                new EventExecution(exA.Event, exA.Tag.Uid),
            };

            for (int i = 0; i < AllValid.GlobalHistory.Length; i++)
            {
                Assert.AreEqual(AllValid.GlobalHistory[i], executionHistory[i]);
            }
        }

        [Test]
        public void PartiallyValidTopOrdering()
        {
            PartiallyValid.CarveConsistentCut();
            PartiallyValid.BuildGraph();
            PartiallyValid.GetCycleTopologicalOrdering();

            var executionHistory = new EventExecution[] {
                new EventExecution(exA.Event, exA.Tag.Uid),
                new EventExecution(exB.Event, exB.Tag.Uid),
            };

            for (int i = 0; i < PartiallyValid.GlobalHistory.Length; i++)
            {
                Assert.AreEqual(PartiallyValid.GlobalHistory[i], executionHistory[i]);
            }
        }

        [Test]
        public void SingleEventMultipleExecutionsTopOrdering()
        {
            SingleEventMultipleExecutions.CarveConsistentCut();
            SingleEventMultipleExecutions.BuildGraph();
            SingleEventMultipleExecutions.GetCycleTopologicalOrdering();

            var executionHistory = new EventExecution[] {
                new EventExecution(exA.Event, exA.Tag.Uid),
                new EventExecution(exA.Event, exA1.Tag.Uid),
                new EventExecution(exA.Event, exA2.Tag.Uid),
            };

            for (int i = 0; i < SingleEventMultipleExecutions.GlobalHistory.Length; i++)
            {
                Assert.AreEqual(SingleEventMultipleExecutions.GlobalHistory[i], executionHistory[i]);
            }
        }
    }
}
