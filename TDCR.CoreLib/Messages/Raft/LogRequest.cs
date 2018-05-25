namespace TDCR.CoreLib.Messages.Raft
{
    public class LogRequest : IPayload<Wire.Raft.LogRequest>
    {
        public Wire.Raft.LogRequest ToWire()
        {
            return new Wire.Raft.LogRequest();
        }

        public static LogRequest FromWire(Wire.Raft.LogRequest wire)
        {
            return new LogRequest();
        }
    }
}
