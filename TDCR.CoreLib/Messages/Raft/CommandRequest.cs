using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Raft
{
    public class CommandRequest : IPayload<Wire.Raft.CommandRequest>
    {
        public CommandTag Tag { get; set; }
        public Uid Event { get; set; }

        public Wire.Raft.CommandRequest ToWire()
        {
            return new Wire.Raft.CommandRequest
            {
                Tag = Tag.ToWire(),
                Event = Event.ToWire()
            };
        }

        public static CommandRequest FromWire(Wire.Raft.CommandRequest wire)
        {
            return new CommandRequest
            {
                Tag = CommandTag.FromWire(wire.Tag),
                Event = Uid.FromWire(wire.Event)
            };
        }
    }
}
