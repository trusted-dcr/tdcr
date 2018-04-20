using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Discovery
{
    public class Hello : IPayload<Wire.Discovery.HelloMsg>
    {
        public uint Nonce { get; set; }
        public uint Version { get; set; }
        public Addr Receiver { get; set; }

        public Wire.Discovery.HelloMsg ToWire()
        {
            return new Wire.Discovery.HelloMsg
            {
                Nonce = Nonce,
                Version = Version,
                Receiver = Receiver.ToWire()
            };
        }

        public static Hello FromWire(Wire.Discovery.HelloMsg wire)
        {
            return new Hello
            {
                Nonce = wire.Nonce,
                Version = wire.Version,
                Receiver = Addr.FromWire(wire.Receiver)
            };
        }
    }
}
