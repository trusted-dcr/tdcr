using System;
using System.Diagnostics;

namespace TDCR.CoreLib.Messages.Network
{
    public class Uid : IPayload<Wire.Network.Uid>
    {
        public ulong Part1 { get; set; }
        public ulong Part2 { get; set; }

        public Wire.Network.Uid ToWire()
        {
            return new Wire.Network.Uid
            {
                Part1 = Part1,
                Part2 = Part2
            };
        }

        public static Uid FromWire(Wire.Network.Uid wire)
        {
            return new Uid
            {
                Part1 = wire.Part1,
                Part2 = wire.Part2
            };
        }

        public static bool operator ==(Uid lhs, Uid rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (lhs is null || rhs is null)
                return false;
            return lhs.Part1 == rhs.Part1 && lhs.Part2 == rhs.Part2;
        }

        public static bool operator !=(Uid lhs, Uid rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            Uid that = obj as Uid;
            if (that == null)
                return false;
            return this == that;
        }

        public override int GetHashCode()
        {
            var hashCode = -1585277143;
            hashCode = hashCode * -1521134295 + Part1.GetHashCode();
            hashCode = hashCode * -1521134295 + Part2.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return BitConverter.ToString(PackLongs(Part1, Part2));
        }

        private static byte[] PackLongs(params ulong[] longs)
        {
            byte[] result = new byte[longs.Length * 8];
            for (int i = 0; i < longs.Length; i++)
                Array.Copy(BitConverter.GetBytes(longs[i]), 0, result, i * 8, 8);
            return result;
        }

        private static ulong UnpackLong(byte[] bytes, int offset)
        {
            Debug.Assert(bytes.Length >= offset + 8);
            ulong result = 0;
            for (int i = 0; i < 8; i++)
                result |= (ulong)bytes[offset + i] << (i * 8);
            return result;
        }

        public Uid()
        {
            var rand = new Random();
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            Part1 = BitConverter.ToUInt64(buf, 0);
            rand.NextBytes(buf);
            Part2 = BitConverter.ToUInt64(buf, 0);
        }

        public Uid(ulong p1, ulong p2)
        {
            Part1 = p1;
            Part2 = p2;
        }
    }
}
