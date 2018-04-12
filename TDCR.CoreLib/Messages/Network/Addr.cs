using System.Diagnostics;
using System.Net;

namespace TDCR.CoreLib.Messages.Network
{
    public class Addr : IPayload<Wire.Network.Addr>
    {
        public IPAddress IP { get; set; }
        public ushort Port { get; set; }

        public Addr() {}

        public Addr(IPEndPoint ep)
        {
            Debug.Assert(ep.Port <= ushort.MaxValue);
            IP = ep.Address;
            Port = (ushort)ep.Port;
        }

        public Wire.Network.Addr ToWire()
        {
            return new Wire.Network.Addr
            {
                Ip = Google.Protobuf.ByteString.CopyFrom(IP.GetAddressBytes()),
                Port = Port
            };
        }

        public static Addr FromWire(Wire.Network.Addr wire)
        {
            return new Addr
            {
                IP = new IPAddress(wire.Ip.ToByteArray()),
                Port = (ushort)wire.Port
            };
        }

        public static bool operator ==(Addr lhs, Addr rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (lhs is null || rhs is null)
                return false;
            return lhs.IP.Equals(rhs.IP) && lhs.Port == rhs.Port;
        }

        public static bool operator !=(Addr lhs, Addr rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            Addr that = obj as Addr;
            if (that == null)
                return false;
            return this == that;
        }

        public override int GetHashCode()
        {
            var hashCode = -910227966;
            hashCode = hashCode * -1521134295 + IP.GetHashCode();
            hashCode = hashCode * -1521134295 + Port.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return new IPEndPoint(IP, Port).ToString();
        }
    }
}
