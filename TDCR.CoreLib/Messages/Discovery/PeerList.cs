using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Discovery
{
    public class PeerList : IPayload<Wire.Discovery.PeerList>
    {
        public Addr[] Peers { get; set; }

        public Wire.Discovery.PeerList ToWire()
        {
            Wire.Discovery.PeerList wire = new Wire.Discovery.PeerList();
            for (int i = 0; i < Peers.Length; i++)
                wire.Peers.Add(Peers[i].ToWire());

            return wire;
        }

        public static PeerList FromWire(Wire.Discovery.PeerList wire)
        {
            Addr[] peers = new Addr[wire.Peers.Count];
            for (int i = 0; i < wire.Peers.Count; i++)
                peers[i] = Addr.FromWire(wire.Peers[i]);

            return new PeerList
            {
                Peers = peers
            };
        }
    }
}
