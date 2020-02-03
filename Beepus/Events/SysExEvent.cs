using System;
using System.Collections.Generic;
using Beepus.Utils;

namespace Beepus.Events
{
    public static class SysExEventFactory
    {
        public static IEvent SysExEvent(byte[] content, int startIndex, byte statusByte, int deltaTime, out int endIndex)
        {
            if (statusByte == 0xF0) // Handle SysEx events
            {
                return new SysExEvent(content, startIndex, deltaTime, out endIndex);
            }
            else if (statusByte == 0xF7) // Handle escape sequences
            {
                return new EscapeSequence(content, startIndex, deltaTime, out endIndex);
            }
            else
            {
                throw new FormatException();
            }
        }
    }

    public class SysExEvent : IEvent
    {
        public int DeltaTime { get; private set; }
        public byte[] Message { get; private set; }

        public SysExEvent(byte[] content, int startIndex, int deltaTime, out int endIndex)
        {
            DeltaTime = deltaTime;

            bool isFinished = false;
            List<byte> bytes = new List<byte>();

            while (!isFinished)
            {
                int length = ByteTools.ConvertVariableLenght(content, startIndex, out int offset);
                startIndex += offset;

                for (int i = 0; i < length; i++)
                {
                    if (i == length - 1 && content[startIndex + i] == 0xF7) // Check if last byte is 0xF7
                    {
                        isFinished = true;
                        break;
                    }

                    bytes.Add(content[startIndex + i]);
                }

                startIndex += length;
            }

            Message = bytes.ToArray();
            endIndex = startIndex;
        }
    }

    public class EscapeSequence : IEvent
    {
        public int DeltaTime { get; private set; }
        public byte[] Bytes { get; private set; }

        public EscapeSequence(byte[] content, int startIndex, int deltaTime, out int endIndex)
        {
            DeltaTime = deltaTime;

            int length = ByteTools.ConvertVariableLenght(content, startIndex, out int offset);
            endIndex = startIndex + offset + length;

            Bytes = new byte[length];
            Array.Copy(content, startIndex + offset, Bytes, 0, length);
        }
    }
}
