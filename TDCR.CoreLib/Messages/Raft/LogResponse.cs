using System.Collections.Generic;
using System.Linq;
using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Raft
{
    public class LogResponse : IPayload<Wire.Raft.LogResponse>
    {
        public bool Success { get; set; }
        public Uid Leader { get; set; }
        public List<Entry> Entries { get; set; }

        public Wire.Raft.LogResponse ToWire()
        {
            Wire.Raft.LogResponse wire = new Wire.Raft.LogResponse
            {
                Success = Success,
                Leader = Leader.ToWire()
            };
            wire.Entries.AddRange(Entries.Select(entry => entry.ToWire()));
            return wire;
        }

        public static LogResponse FromWire(Wire.Raft.LogResponse wire)
        {
            return new LogResponse
            {
                Success = wire.Success,
                Leader = Uid.FromWire(wire.Leader),
                Entries = wire.Entries.Select(entryWire => Entry.FromWire(entryWire)).ToList()
            };
        }
    }
}
