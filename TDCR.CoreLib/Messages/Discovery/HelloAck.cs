namespace TDCR.CoreLib.Messages.Discovery
{
    public class HelloAck : IPayload<Wire.Discovery.HelloAck>
    {
        public uint Nonce { get; set; }

        public Wire.Discovery.HelloAck ToWire()
        {
            return new Wire.Discovery.HelloAck
            {
                Nonce = Nonce
            };
        }

        public static HelloAck FromWire(Wire.Discovery.HelloAck wire)
        {
            return new HelloAck
            {
                Nonce = wire.Nonce
            };
        }
    }
}
