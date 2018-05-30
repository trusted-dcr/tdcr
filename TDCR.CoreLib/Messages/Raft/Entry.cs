using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Raft
{
    public class Entry : IPayload<Wire.Raft.Entry>
    {
        public uint Index { get; set; }
        public uint Term { get; set; }
        public CommandTag Tag { get; set; }
        public Uid Event { get; set; }

        public Wire.Raft.Entry ToWire()
        {
            return new Wire.Raft.Entry
            {
                Event = Event.ToWire(),
                Index = Index,
                Term = Term,
                Tag = Tag.ToWire()
            };
        }

        public static Entry FromWire(Wire.Raft.Entry wire)
        {
            return new Entry
            {
                Event = Uid.FromWire(wire.Event),
                Index = wire.Index,
                Term = wire.Term,
                Tag = CommandTag.FromWire(wire.Tag)
            };
        }
    }
}
