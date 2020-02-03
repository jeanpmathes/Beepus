using System;

namespace Beepus.Utils
{
    static class ByteTools
    {
        public static uint ToUInt32BigEndian(byte[] value, int startIndex)
        {
            byte[] toConvert = new byte[4];
            Array.Copy(value, startIndex, toConvert, 0, 4);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(toConvert);
            }

            return BitConverter.ToUInt32(toConvert, 0);
        }

        public static uint ToUInt24BigEndian(byte[] value, int startIndex)
        {
            return (uint)(value[startIndex] << 16 | value[startIndex + 1] << 8 | value[startIndex + 2]);
        }

        public static ushort ToUInt16BigEndian(byte[] value, int startIndex)
        {
            byte[] toConvert = new byte[2];
            Array.Copy(value, startIndex, toConvert, 0, 2);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(toConvert);
            }

            return BitConverter.ToUInt16(toConvert, 0);
        }

        public static bool CompareContent(byte[] content, int startingIndex, params byte[] bytesToCompare)
        {
            bool equal = true;

            if (startingIndex + bytesToCompare.Length < content.Length) // Check if there are enough entries in contents to compare
            {
                for (int i = 0; i < bytesToCompare.Length; i++)
                {
                    if (content[startingIndex + i] != bytesToCompare[i])
                    {
                        equal = false;
                    }
                }
            }
            else
            {
                equal = false;
            }

            return equal;
        }

        public static int ConvertVariableLenght(byte[] content, int startIndex, out int length)
        {
            length = 1;

            for (int i = 0; i < 4; i++)
            {
                if ((content[startIndex + i] & 0b1000_0000) == 0b1000_0000) // Check if first bit is 1
                {
                    length++;
                }
                else
                {
                    break;
                }
            }

            int result = 0;

            for (int i = 0; i < length; i++)
            {
                result = result << 7;
                result |= content[startIndex + i] & 0b0111_1111;
            }

            return result;
        }
    }
}
