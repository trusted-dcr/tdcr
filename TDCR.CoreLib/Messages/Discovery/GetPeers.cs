namespace TDCR.CoreLib.Messages.Discovery
{
    public class GetPeers : IPayload<Wire.Discovery.GetPeers>
    {
        public uint Amount { get; set; }

        public Wire.Discovery.GetPeers ToWire()
        {
            return new Wire.Discovery.GetPeers
            {
                Amount = Amount
            };
        }

        public static GetPeers FromWire(Wire.Discovery.GetPeers wire)
        {
            return new GetPeers
            {
                Amount = wire.Amount
            };
        }
    }
}
