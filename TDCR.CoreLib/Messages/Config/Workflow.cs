using System.Linq;

namespace TDCR.CoreLib.Messages.Config
{
    public class Workflow : IPayload<Wire.Dcr.Workflow>
    {
        public string Name { get; set; }
        public Event[] Events { get; set; }

        public Wire.Dcr.Workflow ToWire()
        {
            Wire.Dcr.Workflow wire = new Wire.Dcr.Workflow
            {
                Name = Name
            };

            foreach (Event e in Events)
                wire.Events.Add(e.ToWire());

            return wire;
        }

        public static Workflow FromWire(Wire.Dcr.Workflow wire)
        {
            return new Workflow
            {
                Name = wire.Name,
                Events = wire.Events.Select(Event.FromWire).ToArray()
            };
        }
    }
}