using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TDCR.CoreLib.Messages.Network;
using TDCR.CoreLib.Messages.Raft;

namespace TDCR.CoreLib.HistoryCollection
{
    public class CheapShot
    {
        public Tuple<Uid, EventExecution[]>[] Observed { get; private set; }
        public Dictionary<EventExecution, HashSet<EventExecution>> Graph { get; private set; }
        public EventExecution[] GlobalHistory { get; private set; }

        private Dictionary<Uid, HashSet<EventExecution>> EventToExecutions { get; set; }
        private Dictionary<EventExecution, List<Tuple<Uid, int>>> ExecutionsToEvents { get; set; }

        private void CascadingInvalidate(EventExecution evex)
        {
            // Get all instances
            ExecutionsToEvents.TryGetValue(evex, out List<Tuple<Uid, int>> instances);
            foreach (var i in instances)
            {
                var ev = Observed.First(o => o.Item1.Equals(i.Item1)); // Find event which observed invalidated
                for (int j = i.Item2; j < ev.Item2.Length; j++) // For invalidated and all following, set invalid and cascade
                {
                    ev.Item2[j].Valid = false;
                    if (i.Item2 != j) CascadingInvalidate(ev.Item2[j]);
                }
            }
        }

        private void CarveConsistentCut()
        {
            foreach (var ev in Observed)
                foreach (var ex in ev.Item2)
                {
                    if (!ex.Valid) break; // Found invalid, all following must be invalid.

                    // Check if in owners list
                    EventToExecutions.TryGetValue(ex.Event, out HashSet<EventExecution> val);
                    if (val == null || !val.Contains(ex))
                        // Not in owners list. Mark as invalid and go through all instances and set as invalid
                        CascadingInvalidate(ex);
                }
        }

        private void GetCycleTopologicalOrdering()
        {
            var history = new EventExecution[Graph.Keys.Count];
            var idx = Graph.Keys.Count;

            var traversal = new Stack<EventExecution>();

            foreach (var k in Graph.Keys)
            {
                if (k.Marked) continue;

                var node = k;

                while (true)
                {
                    // Mark
                    node.Marked = true;

                    // If unmarked child, traverse
                    Graph.TryGetValue(node, out HashSet<EventExecution> neighbours);
                    foreach (var c in neighbours)
                        if (!c.Marked)
                        {
                            traversal.Push(node);
                            node = c;
                            break;
                        }

                    if (!node.Marked) continue; // Found new child. Continue traversal from it.

                    // Else add to result
                    history[--idx] = node;

                    //Backtrack if able
                    if (traversal.Count == 0) break;
                    node = traversal.Pop();
                }
            }

            GlobalHistory = history;
        }

        private void BuildGraph()
        {
            var graph = new Dictionary<EventExecution, HashSet<EventExecution>>();

            foreach (var ev in Observed)
                for (int j = 0; j < ev.Item2.Length; j++)
                {
                    // If invalid, this sequence is done. Break.
                    if (!ev.Item2[j].Valid) break;

                    // If execution not already in map, add it
                    if (!graph.ContainsKey(ev.Item2[j]))
                        graph.Add(ev.Item2[j], new HashSet<EventExecution>());

                    // If next is valid, add as target
                    if (j + 1 >= ev.Item2.Length || !ev.Item2[j + 1].Valid) break;

                    graph.TryGetValue(ev.Item2[j], out HashSet<EventExecution> val);
                    val.Add(ev.Item2[j + 1]);
                }

            Graph = graph;
        }

        public CheapShot(List<Tuple<Uid, Entry[]>> obs)
        {
            // array af event -> event execution array
            var seenExecutions = new HashSet<EventExecution>(); // Prevent duplicate executions from being created.
            Observed = new Tuple<Uid, EventExecution[]>[obs.Count];
            for (int i = 0; i < obs.Count; i++)
            {
                var execs = obs[i].Item2.Where(e => e.Tag.Type.Equals(CommandTag.CommandType.Exec)).ToArray();

                Observed[i] = new Tuple<Uid, EventExecution[]>(obs[i].Item1, new EventExecution[execs.Length]);
                for (int j = 0; j < execs.Length; j++)
                {
                    var evex = new EventExecution(execs[j].Event, execs[j].Tag.Uid);

                    // Check if already exists. If it does, add existing instead
                    if (seenExecutions.TryGetValue(evex, out EventExecution found))
                    {
                        Observed[i].Item2[j] = found;
                        continue;
                    }

                    // Else save new
                    seenExecutions.Add(evex);
                    Observed[i].Item2[j] = evex;
                }
            }

            // Map of Event -> event execution set
            EventToExecutions = new Dictionary<Uid, HashSet<EventExecution>>();
            foreach(var ev in Observed)
                EventToExecutions.Add(ev.Item1, new HashSet<EventExecution>(ev.Item2));

            // Create map of event execution => List of (Event, Index)
            ExecutionsToEvents = new Dictionary<EventExecution, List<Tuple<Uid, int>>>();
            for (int i = 0; i < Observed.Length; i++)
            {
                for (int j = 0; j < Observed[i].Item2.Length; j++)
                {
                    // If not already in map, add it
                    if (!ExecutionsToEvents.ContainsKey(Observed[i].Item2[j]))
                        ExecutionsToEvents.Add(Observed[i].Item2[j], new List<Tuple<Uid, int>> { new Tuple<Uid, int>(Observed[i].Item1, j) });
                    else // Add this location to the value
                    {
                        ExecutionsToEvents.TryGetValue(Observed[i].Item2[j], out List<Tuple<Uid, int>> val);
                        val.Add(new Tuple<Uid, int>(Observed[i].Item1, j));
                    }
                }
            }

            // Run the Algorithm
            // Consistent cut
            CarveConsistentCut();

            // Convert to edges
            BuildGraph();

            // Modified topsort
            GetCycleTopologicalOrdering();
        }
    }
}
