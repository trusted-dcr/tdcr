using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TDCR.CoreLib.Messages.Network;
using Newtonsoft.Json;

namespace TDCR.CoreLib.Messages.Config
{
    public class Peer
    {
        public Uid UID { get; set; }
        public Uid Event { get; set; }
        public IPEndPoint Address { get; set; }

        [JsonConstructor]
        public Peer(Uid uid, Uid @event, string address)
        {
            UID = uid;
            Event = @event;
            //Address = new IPEndPoint ()
        }
    }
}
