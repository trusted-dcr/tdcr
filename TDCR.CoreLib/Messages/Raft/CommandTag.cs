using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Raft
{
    public class CommandTag : IPayload<Wire.Raft.CommandTag>
    {
        public enum CommandType
        {
            Lock = 0,
            Abort = 1,
            Exec = 2
        }

        public Uid Uid { get; set; }
        public CommandType Type { get; set; }

        public Wire.Raft.CommandTag ToWire()
        {
            return new Wire.Raft.CommandTag
            {
                Uid = Uid.ToWire(),
                Type = (Wire.Raft.CommandType)Type
            };
        }

        public static CommandTag FromWire(Wire.Raft.CommandTag wire)
        {
            return new CommandTag
            {
                Uid = Uid.FromWire(wire.Uid),
                Type = (CommandType)wire.Type
            };
        }
    }
}
