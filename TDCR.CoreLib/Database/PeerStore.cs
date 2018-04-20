using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Database
{
    public class PeerStore : FileStore
    {
        public static PeerStore Instance { get; } = new PeerStore();

        private PeerStore() : base("peers.dat") {}

        public Addr[] ReadAllPeers()
        {
            List<Addr> peers = new List<Addr>();
            using (FileStream file = OpenFile())
            {
                while (file.Position < file.Length)
                {
                    Wire.Network.Addr wire = Wire.Network.Addr.Parser.ParseDelimitedFrom(file);
                    Addr peer = Addr.FromWire(wire);
                    peers.Add(peer);
                }
            }
            return peers.ToArray();
        }

        public void AppendPeers(Addr[] peers)
        {
            using (FileStream file = OpenFile())
            {
                file.Seek(0, SeekOrigin.End);
                foreach (Addr peer in peers)
                {
                    Wire.Network.Addr wire = peer.ToWire();
                    wire.WriteDelimitedTo(file);
                }
            }
        }
    }
}
