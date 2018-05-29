using System.Net;
using TDCR.CoreLib.Messages.Network;
using Newtonsoft.Json;

namespace TDCR.CoreLib.Messages.Config
{
    public class Peer : IPayload<Wire.Sgx.SgxConfig.Types.Peer>
    {
        public Uid Uid { get; set; }
        public Uid Event { get; set; }
        public Addr Addr { get; set; }

        public Peer() {}

        [JsonConstructor]
        public Peer(Uid uid, Uid @event, string addr)
        {
            Uid = uid;
            Event = @event;

            // only supports IPv4
            string[] split = addr.Split(':');
            Addr = new Addr
            {
                EndPoint = new IPEndPoint(IPAddress.Parse(split[0]), int.Parse(split[1]))
            };
        }

        public Wire.Sgx.SgxConfig.Types.Peer ToWire()
        {
            return new Wire.Sgx.SgxConfig.Types.Peer
            {
                Uid = Uid.ToWire(),
                Event = Event.ToWire(),
                Addr = Addr.ToWire()
            };
        }

        public static Peer FromWire(Wire.Sgx.SgxConfig.Types.Peer wire)
        {
            return new Peer
            {
                Uid = Uid.FromWire(wire.Uid),
                Event = Uid.FromWire(wire.Event),
                Addr = Addr.FromWire(wire.Addr)
            };
        }
    }
}
