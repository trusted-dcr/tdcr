using System;
using System.Collections.Generic;
using System.Text;
using TDCR.CoreLib.Messages.Network;

namespace TDCR.CoreLib.HistoryCollection
{
    public class EventExecution
    {
        public Uid Event { get; set; }
        public Uid ExecutionID { get; set; }

        public bool Valid { get; set; }
        public bool Marked { get; set; }

        public EventExecution(Uid ev, Uid ex, bool valid = true)
        {
            Event = ev;
            ExecutionID = ex;
            Valid = valid;
            Marked = false;
        }

        public static bool operator ==(EventExecution lhs, EventExecution rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (lhs is null || rhs is null)
                return false;
            return lhs.Event.Equals(rhs.Event) && lhs.ExecutionID.Equals(rhs.ExecutionID);
        }

        public static bool operator !=(EventExecution lhs, EventExecution rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EventExecution))
                return false;

            var ex = (EventExecution)obj;
            return ex.Event.Equals(Event) && ex.ExecutionID.Equals(ExecutionID);
        }

        public override int GetHashCode()
        {
            var hashCode = -2125768153;
            hashCode = hashCode * -1521134295 + Event.GetHashCode();
            hashCode = hashCode * -1521134295 + ExecutionID.GetHashCode();
            return hashCode;
        }
    }
}
