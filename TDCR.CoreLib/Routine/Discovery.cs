using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using TDCR.CoreLib.Network;
using TDCR.CoreLib.Messages.Network;
using TDCR.CoreLib.Messages.Discovery;

namespace TDCR.CoreLib.Routine
{
    public class Discovery : IRoutine
    {
        public HashSet<Addr> KnownPeers { get; set; }

        private IRouter router;
        private ILogger logger;
        private Random rng;

        public Discovery(ICollection<Addr> knownPeers)
        {
            KnownPeers = new HashSet<Addr>(knownPeers);
            rng = new Random();
        }

        public void Attach(IRouter router, ILogger logger = null)
        {
            this.router = router;
            this.logger = logger;

            router.Connected += OnConnected;
            router.RecvGetPeers += OnRecvGetPeers;
            router.RecvPeerList += OnRecvPeerList;
        }

        public void GetPeers(Uid target)
        {

        }

        private void OnConnected(Uid peer, Addr addr, IRouter _)
        {
            logger?.Debug($"Requesting peers from {peer}");
            router.SendAsync(peer, new GetPeers
            {
                Amount = Constants.GetPeerAmount
            });
        }

        private void OnRecvGetPeers(Uid sender, GetPeers payload, IRouter _)
        {
            logger?.Info($"Got request for {payload.Amount} peers from {sender}");

            Addr[] peers;
            if (KnownPeers.Count <= payload.Amount)
            {
                logger?.Info($"Too few known peers to fully comply ({KnownPeers.Count}/{payload.Amount})");
                peers = KnownPeers.ToArray();
            }

            int overflow = KnownPeers.Count - (int)payload.Amount;
            List<Addr> plist = new List<Addr>(KnownPeers);
            for (int i = 0; i < overflow; i++)
                plist.RemoveAt(rng.Next(plist.Count));
            peers = plist.ToArray();

            logger?.Info($"Sending {peers.Length} peers to {sender}");
            router.SendAsync(sender, new PeerList
            {
                Peers = peers
            });
        }

        private void OnRecvPeerList(Uid sender, PeerList payload, IRouter _)
        {
            logger?.Info($"Got {payload.Peers.Length} peers from {sender}");
            foreach (Addr peer in payload.Peers)
                KnownPeers.Add(peer);
        }
    }
}
