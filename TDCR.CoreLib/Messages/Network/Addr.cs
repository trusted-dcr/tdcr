using System;
using System.Net;

namespace TDCR.CoreLib.Messages.Network
{
    public class Addr : IPayload<Wire.Network.Addr>
    {
        public IPEndPoint EndPoint { get; set; }

        public Wire.Network.Addr ToWire()
        {
            byte[] ipPadded = new byte[8];
            byte[] ipBytes = EndPoint.Address.GetAddressBytes();
            Array.Copy(ipBytes, ipPadded, ipBytes.Length);

            return new Wire.Network.Addr
            {
                Ip = BinaryHelpers.UnpackUInt64(ipPadded),
                Port = (ushort)EndPoint.Port
            };
        }

        public static Addr FromWire(Wire.Network.Addr wire)
        {
            return new Addr
            {
                EndPoint = new IPEndPoint(
                    new IPAddress(BinaryHelpers.PackUInt64(wire.Ip)),
                    (ushort)wire.Port)
            };
        }

        public static bool operator ==(Addr lhs, Addr rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (lhs is null || rhs is null)
                return false;
            return lhs.EndPoint.Address.Equals(rhs.EndPoint.Address)
                && lhs.EndPoint.Port == rhs.EndPoint.Port;
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
            return EndPoint.GetHashCode();
        }

        public override string ToString()
        {
            return EndPoint.ToString();
        }
    }
}
