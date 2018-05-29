using System.Linq;
using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.Messages.Config
{
    public class Event : IPayload<Wire.Dcr.Event>
    {
        public Uid Uid { get; set; }
        public string Name { get; set; }

        public bool Executed { get; set; }
        public bool Excluded { get; set; }
        public bool Pending { get; set; }

        public Uid[] ConditionRelations { get; set; }
        public Uid[] MilestoneRelations { get; set; }
        public Uid[] ExcludeRelations { get; set; }
        public Uid[] IncludeRelations { get; set; }
        public Uid[] PendingRelations { get; set; }

        public bool Included {
            get => !Excluded;
            set => Excluded = !value;
        }

        public Wire.Dcr.Event ToWire()
        {
            Wire.Dcr.Event wire = new Wire.Dcr.Event
            {
                Uid = Uid.ToWire(),
                Name = Name,

                Executed = Executed,
                Excluded = Excluded,
                Pending = Pending
            };

            foreach (Uid r in ConditionRelations)
                wire.ConditionRelations.Add(r.ToWire());
            foreach (Uid r in MilestoneRelations)
                wire.MilestoneRelations.Add(r.ToWire());
            foreach (Uid r in ExcludeRelations)
                wire.ExcludeRelations.Add(r.ToWire());
            foreach (Uid r in IncludeRelations)
                wire.IncludeRelations.Add(r.ToWire());
            foreach (Uid r in PendingRelations)
                wire.PendingRelations.Add(r.ToWire());

            return wire;
        }

        public static Event FromWire(Wire.Dcr.Event wire)
        {
            return new Event
            {
                Uid = Uid.FromWire(wire.Uid),
                Name = wire.Name,

                Executed = wire.Executed,
                Excluded = wire.Excluded,
                Pending = wire.Pending,

                ConditionRelations = wire.ConditionRelations.Select(Uid.FromWire).ToArray(),
                MilestoneRelations = wire.MilestoneRelations.Select(Uid.FromWire).ToArray(),
                ExcludeRelations = wire.ExcludeRelations.Select(Uid.FromWire).ToArray(),
                IncludeRelations = wire.IncludeRelations.Select(Uid.FromWire).ToArray(),
                PendingRelations = wire.PendingRelations.Select(Uid.FromWire).ToArray()
            };
        }
    }
}
