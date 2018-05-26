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
        public Dictionary<EventExecution, List<EventExecution>> Graph { get; set; }
        public EventExecution[] GlobalHistory { get; set; }
        
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

        public void CarveConsistentCut()
        {
            foreach (var ev in Observed)
                foreach (var ex in ev.Item2)
                {
                    if (!ex.Valid) break; // Found invalid, all following must be invalid.

                    // Check if in owners list
                    EventToExecutions.TryGetValue(ex.Event, out HashSet<EventExecution> val);
                    if (!val.Contains(ex))
                        // Not in owners list. Mark as invalid and go through all instances and set as invalid
                        CascadingInvalidate(ex);
                }
        }

        public void GetCycleTopologicalOrdering()
        {
            var history = new EventExecution[Graph.Keys.Count];
            var idx = Graph.Keys.Count-1;
            
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
                    Graph.TryGetValue(node, out List<EventExecution> neighbours);
                    foreach (var c in neighbours)
                        if (!c.Marked)
                        {
                            traversal.Push(node);
                            node = c;
                            break;
                        }

                    if (!node.Marked) continue; // Found new child. Continue traversal from it.

                    // Else add to result
                    history[idx--] = node;

                    //Backtrack if able
                    if (traversal.Count == 0) break;
                    node = traversal.Pop();
                }
            }

            GlobalHistory = history;
        }

        public void BuildGraph()
        {
            var graph = new Dictionary<EventExecution, List<EventExecution>>();

            foreach (var ev in Observed)
                for (int j = 0; j < ev.Item2.Length; j++)
                {
                    // If invalid, this sequence is done. Break.
                    if (!ev.Item2[j].Valid) break;

                    // If execution not already in map, add it
                    if (!graph.ContainsKey(ev.Item2[j]))
                        graph.Add(ev.Item2[j], new List<EventExecution>());

                    // If next is valid, add as target
                    if (j + 1 >= ev.Item2.Length || !ev.Item2[j + 1].Valid) break;

                    graph.TryGetValue(ev.Item2[j], out List<EventExecution> val);
                    val.Add(ev.Item2[j + 1]);
                }

            Graph = graph;
        }

        public CheapShot(List<Tuple<Uid, Entry[]>> obs)
        {
            // array af event -> event execution array
            Observed = new Tuple<Uid, EventExecution[]>[obs.Count];
            for (int i = 0; i < obs.Count; i++)
                Observed[i] = new Tuple<Uid, EventExecution[]>(obs[i].Item1,
                    obs[i].Item2.Select(entry => new EventExecution(entry.Event, entry.Tag.Uid)).ToArray());

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
        }

        public EventExecution[] CollectHistory()
        {
            // Consistent cut
            CarveConsistentCut();

            // Convert to edges
            BuildGraph();

            // Modified topsort
            GetCycleTopologicalOrdering();
            
            return GlobalHistory;
        }
    }
}
