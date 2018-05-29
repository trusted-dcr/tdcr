using System.Linq;
using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Config
{
    public class SgxConfig : IPayload<Wire.Sgx.SgxConfig>
    {
        public Uid Self { get; set; }
        public Workflow Workflow { get; set; }
        public Peer[] Peers { get; set; }

        public Wire.Sgx.SgxConfig ToWire()
        {
            Wire.Sgx.SgxConfig wire = new Wire.Sgx.SgxConfig
            {
                Self = Self.ToWire(),
                Workflow = Workflow.ToWire()
            };

            foreach (Peer p in Peers)
                wire.Peers.Add(p.ToWire());

            return wire;
        }

        public static SgxConfig FromWire(Wire.Sgx.SgxConfig wire)
        {
            return new SgxConfig
            {
                Self = Uid.FromWire(wire.Self),
                Workflow = Workflow.FromWire(wire.Workflow),
                Peers = wire.Peers.Select(Peer.FromWire).ToArray()
            };
        }
    }
}
