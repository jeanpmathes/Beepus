using System;
using System.IO;

namespace Beepus.Utils
{
    static class ByteTools
    {
        public static uint ReadUInt32BigEndian(Stream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            
            return BitConverter.ToUInt32(buffer, 0);
        }

        public static uint ReadUInt24BigEndian(Stream stream)
        {
            var buffer = new byte[3];
            stream.Read(buffer, 0, 3);
            
            return (uint)(buffer[0] << 16 | buffer[1] << 8 | buffer[2]);
        }

        public static ushort ReadUInt16BigEndian(Stream stream)
        {
            var buffer = new byte[2];
            stream.Read(buffer, 0, 2);
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            return BitConverter.ToUInt16(buffer, 0);
        }

        public static bool CompareContent(Stream stream, params byte[] bytesToCompare)
        {
            var isEqual = true;
            
            foreach (var b in bytesToCompare)
            {
                var current = (byte) stream.ReadByte();

                if (b != current)
                {
                    isEqual = false;
                }
            }

            return isEqual;
        }

        public static int ReadVariableLenght(Stream stream)
        {
            int b;
            var result = 0;

            do
            {
                b = stream.ReadByte();
                result <<= 7;
                result |= b & 0b0111_1111;
            }
            while ((b & 0b1000_0000) == 0b1000_0000); // Check if first bit is 1

            return result;
        }
    }
}
