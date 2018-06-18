using System.Linq;

namespace TDCR.CoreLib.Messages.Raft
{
    public class AppendRequest : IPayload<Wire.Raft.AppendRequest>
    {
        public uint Term { get; set; }
        public uint PrevTerm { get; set; }
        public uint PrevIndex { get; set; }
        public Entry[] Entries { get; set; }
        public uint CommitIndex { get; set; }

        public Wire.Raft.AppendRequest ToWire()
        {
            Wire.Raft.AppendRequest wire = new Wire.Raft.AppendRequest
            {
                Term = Term,
                PrevTerm = PrevTerm,
                PrevIndex = PrevIndex,
                CommitIndex = CommitIndex
            };
            wire.Entries.AddRange(Entries.Select(entry => entry.ToWire()));
            return wire;
        }
    }
}
