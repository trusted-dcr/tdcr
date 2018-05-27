using System;
using System.Collections.Generic;
using System.Text;
using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Config
{
    public class Event
    {
        public Uid UID { get; set; }
        public string Name { get; set; }

        public Peer[] Cluster { get; set; }

        public bool Executed { get; set; }
        public bool Included { get; set; }
        public bool Pending { get; set; }
        
        public Uid[] ConditionRelations { get; set; }
        public Uid[] MilestoneRelations { get; set; }
        public Uid[] ExcludeRelations { get; set; }
        public Uid[] IncludeRelations { get; set; }
        public Uid[] PendingRelations { get; set; }
    }
}
