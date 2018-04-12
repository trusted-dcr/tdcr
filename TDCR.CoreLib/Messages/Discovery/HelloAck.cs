namespace TDCR.CoreLib.Messages.Discovery
{
    public class HelloAck : IPayload<Wire.Discovery.HelloAck>
    {
        public Wire.Discovery.HelloAck ToWire()
        {
            return new Wire.Discovery.HelloAck();
        }

        public static HelloAck FromWire(Wire.Discovery.HelloAck wire)
        {
            return new HelloAck();
        }
    }
}
