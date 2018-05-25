using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Raft
{
    public class CommandReponse : IPayload<Wire.Raft.CommandResponse>
    {
        public CommandTag Tag { get; set; }
        public bool Success { get; set; }
        public Uid Leader { get; set; }

        public Wire.Raft.CommandResponse ToWire()
        {
            return new Wire.Raft.CommandResponse
            {
                Tag = Tag.ToWire(),
                Success = Success,
                Leader = Leader.ToWire()
            };
        }

        public static CommandReponse FromWire(Wire.Raft.CommandResponse wire)
        {
            return new CommandReponse
            {
                Tag = CommandTag.FromWire(wire.Tag),
                Success = wire.Success,
                Leader = Uid.FromWire(wire.Leader)
            };
        }
    }
}
