using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Discovery
{
    public class Hello : IPayload<Wire.Discovery.HelloMsg>
    {
        public uint Version { get; set; }
        public Addr Receiver { get; set; }

        public Wire.Discovery.HelloMsg ToWire()
        {
            return new Wire.Discovery.HelloMsg
            {
                Version = Version,
                Receiver = Receiver.ToWire()
            };
        }

        public static Hello FromWire(Wire.Discovery.HelloMsg wire)
        {
            return new Hello
            {
                Version = wire.Version,
                Receiver = Addr.FromWire(wire.Receiver)
            };
        }
    }
}
