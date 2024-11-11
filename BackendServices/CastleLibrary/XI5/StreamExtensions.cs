using System;
using System.IO;

namespace XI5
{
    internal static class StreamExtensions
    {
        public static uint ReadUInt(this Stream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }
        
        public static ulong ReadULong(this Stream stream)
        {
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static ushort ReadUShort(this Stream stream)
        {
            byte[] buffer = new byte[2];
            stream.Read(buffer, 0, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);
            return BitConverter.ToUInt16(buffer, 0);
        }
        
        public static bool ReadAll(this Stream stream, byte[] buffer, int startIndex, int count)
        {
            if (stream == null)
                return false;

            int offset = 0;
            while (offset < count)
            {
                int readCount = stream.Read(buffer, startIndex + offset, count - offset);
                if (readCount == 0)
                    return false;
                offset += readCount;
            }
            return true;
        }
    }
}
