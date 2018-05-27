using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Config
{
    public class Peer
    {
        public Uid UID { get; set; }
        public Uid Event { get; set; }
        public IPAddress Address { get; set; }
    }
}
