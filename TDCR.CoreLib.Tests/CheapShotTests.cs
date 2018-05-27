using NUnit.Framework;
using System;
using System.Collections.Generic;
using TDCR.CoreLib.HistoryCollection;
using System.Text;
using TDCR.CoreLib.Messages.Raft;
using TDCR.CoreLib.Messages.Network;
using System.Linq;

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
        readonly static Entry exE = new Entry { Event = new Uid(), Tag = new CommandTag { Type = CommandTag.CommandType.Exec, Uid = new Uid() } };

        readonly static EventExecution eea = new EventExecution(exA.Event, exA.Tag.Uid);
        readonly static EventExecution eea1 = new EventExecution(exA.Event, exA1.Tag.Uid);
        readonly static EventExecution eea2 = new EventExecution(exA.Event, exA2.Tag.Uid);
        readonly static EventExecution eeb = new EventExecution(exB.Event, exB.Tag.Uid);
        readonly static EventExecution eec = new EventExecution(exC.Event, exC.Tag.Uid);
        readonly static EventExecution eed = new EventExecution(exD.Event, exD.Tag.Uid);
        readonly static EventExecution eee = new EventExecution(exE.Event, exE.Tag.Uid);

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

        readonly CheapShot Branching = new CheapShot(new List<Tuple<Uid, Entry[]>>
        {
                new Tuple<Uid, Entry[]>(exA.Event, new Entry[] { exA }), // A
                new Tuple<Uid, Entry[]>(exB.Event, new Entry[] { exA, exB }), // B
                new Tuple<Uid, Entry[]>(exC.Event, new Entry[] { exB, exC }), // C
                new Tuple<Uid, Entry[]>(exD.Event, new Entry[] { exB, exD }), // D
                new Tuple<Uid, Entry[]>(exE.Event, new Entry[] { exC, exD, exE }), // E
        });

        readonly CheapShot Cycle = new CheapShot(new List<Tuple<Uid, Entry[]>>
            {
                new Tuple<Uid, Entry[]>(exA.Event, new Entry[] { exB, exC, exA }), // A
                new Tuple<Uid, Entry[]>(exB.Event, new Entry[] { exC, exB, exA }), // B
                new Tuple<Uid, Entry[]>(exC.Event, new Entry[] { exA, exB, exC }), // C
            });

        [Test]
        public void AllInvalidConsistentCutTest()
        {
            AllInvalid.CarveConsistentCut();

            Assert.False(AllInvalid.Observed[0].Item2[0].Valid);
            Assert.False(AllInvalid.Observed[0].Item2[1].Valid);
            Assert.False(AllInvalid.Observed[1].Item2[0].Valid);
            Assert.False(AllInvalid.Observed[1].Item2[1].Valid);
            Assert.False(AllInvalid.Observed[2].Item2[0].Valid);
            Assert.False(AllInvalid.Observed[2].Item2[1].Valid);
        }

        [Test]
        public void AllInvalidBuildGraphTest()
        {
            AllInvalid.CarveConsistentCut();
            AllInvalid.BuildGraph();
            var graph = AllInvalid.Graph;

            Assert.AreEqual(graph.Keys.Count, 0);
        }

        [Test]
        public void AllInvalidTopOrdering()
        {
            AllInvalid.CollectHistory();

            Assert.AreEqual(AllInvalid.GlobalHistory.Length, 0);
        }

        [Test]
        public void AllValidConsistentCutTest()
        {
            AllValid.CarveConsistentCut();

            Assert.That(AllValid.Observed[0].Item2[0].Valid);
            Assert.That(AllValid.Observed[0].Item2[1].Valid);
            Assert.That(AllValid.Observed[1].Item2[0].Valid);
            Assert.That(AllValid.Observed[1].Item2[1].Valid);
            Assert.That(AllValid.Observed[2].Item2[0].Valid);
            Assert.That(AllValid.Observed[2].Item2[1].Valid);
            Assert.That(AllValid.Observed[3].Item2[0].Valid);
        }
        
        [Test]
        public void AllValidBuildGraphTest()
        {
            AllValid.CarveConsistentCut();
            AllValid.BuildGraph();

            AllValid.Graph.TryGetValue(eea, out HashSet<EventExecution> nodeA);
            AllValid.Graph.TryGetValue(eeb, out HashSet<EventExecution> nodeB);
            AllValid.Graph.TryGetValue(eec, out HashSet<EventExecution> nodeC);
            AllValid.Graph.TryGetValue(eed, out HashSet<EventExecution> nodeD);

            Assert.AreEqual(AllValid.Graph.Keys.Count, 4);
            Assert.AreEqual(nodeA.Count, 0);
            Assert.AreEqual(nodeB.Count, 1);
            Assert.That(nodeB.Contains(eea));
            Assert.AreEqual(nodeC.Count, 1);
            Assert.That(nodeC.Contains(eeb));
            Assert.AreEqual(nodeD.Count, 1);
            Assert.That(nodeD.Contains(eec));
        }

        [Test]
        public void AllValidTopOrdering()
        {
            AllValid.CollectHistory();

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
        public void PartiallyValidConsistentCutTest()
        {
            PartiallyValid.CarveConsistentCut();

            Assert.That(PartiallyValid.Observed[0].Item2[0].Valid);
            Assert.That(PartiallyValid.Observed[0].Item2[1].Valid);
            Assert.That(PartiallyValid.Observed[1].Item2[0].Valid);
            Assert.False(PartiallyValid.Observed[1].Item2[1].Valid);
            Assert.False(PartiallyValid.Observed[2].Item2[0].Valid);
            Assert.False(PartiallyValid.Observed[2].Item2[1].Valid);
        }

        [Test]
        public void PartiallyValidBuildGraphTest()
        {
            PartiallyValid.CarveConsistentCut();
            PartiallyValid.BuildGraph();

            PartiallyValid.Graph.TryGetValue(eea, out HashSet<EventExecution> nodeA);
            PartiallyValid.Graph.TryGetValue(eeb, out HashSet<EventExecution> nodeB);
            PartiallyValid.Graph.TryGetValue(eec, out HashSet<EventExecution> nodeC);

            Assert.AreEqual(PartiallyValid.Graph.Keys.Count, 2);
            Assert.AreEqual(nodeA.Count, 1);
            Assert.That(nodeA.Contains(eeb));
            Assert.AreEqual(nodeB.Count, 0);
        }

        [Test]
        public void PartiallyValidTopOrdering()
        {
            PartiallyValid.CollectHistory();

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
        public void SingleEventMultipleExecutionsConsistentCutTest()
        {
            SingleEventMultipleExecutions.CarveConsistentCut();

            Assert.That(SingleEventMultipleExecutions.Observed[0].Item2[0].Valid);
            Assert.That(SingleEventMultipleExecutions.Observed[0].Item2[1].Valid);
            Assert.That(SingleEventMultipleExecutions.Observed[0].Item2[2].Valid);
        }

        [Test]
        public void SingleEventMultipleExecutionBuildGraphTest()
        {
            SingleEventMultipleExecutions.CarveConsistentCut();
            SingleEventMultipleExecutions.BuildGraph();

            SingleEventMultipleExecutions.Graph.TryGetValue(eea, out HashSet<EventExecution> nodeA);
            SingleEventMultipleExecutions.Graph.TryGetValue(eea1, out HashSet<EventExecution> nodeA1);
            SingleEventMultipleExecutions.Graph.TryGetValue(eea2, out HashSet<EventExecution> nodeA2);

            Assert.AreEqual(SingleEventMultipleExecutions.Graph.Keys.Count, 3);
            Assert.AreEqual(nodeA.Count, 1);
            Assert.AreEqual(nodeA1.Count, 1);
            Assert.AreEqual(nodeA2.Count, 0);
        }
        
        [Test]
        public void SingleEventMultipleExecutionsTopOrdering()
        {
            SingleEventMultipleExecutions.CollectHistory();

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
        
        [Test]
        public void BranchingConsistenCutTest()
        {
            Branching.CarveConsistentCut();

            for (int i = 0; i < Branching.Observed.Length; i++)
                for (int j = 0; j < Branching.Observed[i].Item2.Length; j++)
                    Assert.That(Branching.Observed[i].Item2[j].Valid);
        }

        [Test]
        public void BranchingBuildGraphTest()
        {
            Branching.CarveConsistentCut();
            Branching.BuildGraph();
            
            Branching.Graph.TryGetValue(eea, out HashSet<EventExecution> nodeA);
            Branching.Graph.TryGetValue(eeb, out HashSet<EventExecution> nodeB);
            Branching.Graph.TryGetValue(eec, out HashSet<EventExecution> nodeC);
            Branching.Graph.TryGetValue(eed, out HashSet<EventExecution> nodeD);
            Branching.Graph.TryGetValue(eee, out HashSet<EventExecution> nodeE);

            Assert.AreEqual(nodeA.Count, 1);
            Assert.AreEqual(nodeB.Count, 2);
            Assert.AreEqual(nodeC.Count, 1);
            Assert.AreEqual(nodeD.Count, 1);
            Assert.AreEqual(nodeE.Count, 0);

            Assert.That(nodeA.Contains(eeb));
            Assert.That(nodeB.Contains(eec));
            Assert.That(nodeB.Contains(eed));
            Assert.That(nodeC.Contains(eed));
            Assert.That(nodeD.Contains(eee));
        }

        [Test]
        public void BranchingTopOrderingTest()
        {
            Branching.CollectHistory();

            var executionHistory1 = new EventExecution[]
            {
                eea, eeb, eec, eed, eee
            };

            var executionHistory2 = new EventExecution[]
            {
                eea, eeb, eed, eec, eee
            };

            Assert.AreEqual(Branching.GlobalHistory.Distinct().Count(), executionHistory1.Length);

            for (int i = 0; i < Branching.GlobalHistory.Length; i++)
            {
                Assert.That(Branching.GlobalHistory[i].Equals(executionHistory1[i]) ||
                    Branching.GlobalHistory[i].Equals(executionHistory2[i]));
            }

        }

        [Test]
        public void CycleConsistenCutTest()
        {
            Cycle.CarveConsistentCut();

            for (int i = 0; i < Cycle.Observed.Length; i++)
                for (int j = 0; j < Cycle.Observed[i].Item2.Length; j++)
                    Assert.That(Cycle.Observed[i].Item2[j].Valid);
        }

        [Test]
        public void CycleBuildGraphTest()
        {
            Cycle.CarveConsistentCut();
            Cycle.BuildGraph();
            
            Cycle.Graph.TryGetValue(eea, out HashSet<EventExecution> nodeA);
            Cycle.Graph.TryGetValue(eeb, out HashSet<EventExecution> nodeB);
            Cycle.Graph.TryGetValue(eec, out HashSet<EventExecution> nodeC);

            Assert.AreEqual(nodeA.Count, 1);
            Assert.AreEqual(nodeB.Count, 2);
            Assert.AreEqual(nodeC.Count, 2);

            Assert.That(nodeA.Contains(eeb));
            Assert.That(nodeB.Contains(eec));
            Assert.That(nodeB.Contains(eea));
            Assert.That(nodeC.Contains(eea));
            Assert.That(nodeC.Contains(eeb));
        }

        [Test]
        public void CycleTopOrderingTest()
        {
            Cycle.CollectHistory();

            Assert.AreEqual(Cycle.GlobalHistory.Length, 3);
            Assert.AreEqual(Cycle.GlobalHistory.Distinct().Count(), 3);
            Assert.That(Cycle.GlobalHistory.Contains(eea));
            Assert.That(Cycle.GlobalHistory.Contains(eeb));
            Assert.That(Cycle.GlobalHistory.Contains(eec));
        }
    }
}
