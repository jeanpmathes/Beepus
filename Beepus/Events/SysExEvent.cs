using System;
using System.Collections.Generic;
using System.IO;
using Beepus.Utils;

namespace Beepus.Events
{
    public static class SysExEventFactory
    {
        public static IEvent SysExEvent(FileStream stream, byte statusByte, int deltaTime)
        {
            if (statusByte == 0xF0) // Handle SysEx events
            {
                return new SysExEvent(stream, deltaTime);
            }
            else if (statusByte == 0xF7) // Handle escape sequences
            {
                return new EscapeSequence(stream, deltaTime);
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

        public SysExEvent(FileStream stream, int deltaTime)
        {
            DeltaTime = deltaTime;

            var isFinished = false;
            var bytes = new List<byte>();

            while (!isFinished)
            {
                int length = ByteTools.ReadVariableLenght(stream);

                for (var i = 0; i < length; i++)
                {
                    var b = (byte) stream.ReadByte();
                    
                    if (i == length - 1 && b == 0xF7) // Check if last byte is 0xF7
                    {
                        isFinished = true;
                        break;
                    }

                    bytes.Add(b);
                }
            }

            Message = bytes.ToArray();
        }
    }

    public class EscapeSequence : IEvent
    {
        public int DeltaTime { get; private set; }
        public byte[] Bytes { get; private set; }

        public EscapeSequence(FileStream stream, int deltaTime)
        {
            DeltaTime = deltaTime;

            int length = ByteTools.ReadVariableLenght(stream);
            
            Bytes = new byte[length];
            stream.Read(Bytes, 0, length);
        }
    }
}
