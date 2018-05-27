using System;
using System.Collections.Generic;
using System.Text;
using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Config
{
    public class Graph
    {
        public Uid UID { get; set; }

        public Event[] Events { get; set; }
    }
}
