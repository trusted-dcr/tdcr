using System;
using System.Diagnostics;

namespace TDCR.CoreLib.Messages
{
    static class BinaryHelpers
    {
        public static byte[] PackUInt64(params ulong[] longs)
        {
            byte[] result = new byte[longs.Length * 8];
            for (int i = 0; i < longs.Length; i++)
                Array.Copy(BitConverter.GetBytes(longs[i]), 0, result, i * 8, 8);
            return result;
        }

        public static ulong UnpackUInt64(byte[] bytes, int offset = 0)
        {
            Debug.Assert(bytes.Length >= offset + 8);
            ulong result = 0;
            for (int i = 0; i < 8; i++)
                result |= (ulong)bytes[offset + i] << (i * 8);
            return result;
        }
    }
}
