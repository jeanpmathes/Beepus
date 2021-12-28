using Beepus.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beepus.Utils;

namespace Beepus
{
    public enum EventType
    {
        Midi,
        SysEx,
        Meta
    }

    public static class EventReader
    {
        public static IEvent ReadEvent(Stream stream, out EventType type, out byte statusByte, byte lastStatus = 0x00)
        {
            var deltaTime = ByteTools.ReadVariableLenght(stream);
            statusByte = (byte) stream.ReadByte();

            if (statusByte < 0x80) // Check for running status
            {
                if (lastStatus == 0x00)
                {
                    throw new FormatException($"Running status found but no correct status!");
                }

                statusByte = lastStatus;
                stream.Seek(stream.Position - 1, SeekOrigin.Begin);
            }

            if (statusByte >= 0x80 && statusByte <= 0xEF) // Check for MIDI events
            {
                type = EventType.Midi;
                Console.WriteLine("MIDI event found...");
                Console.WriteLine($"Status byte: {statusByte:X}, deltaTime: {deltaTime}");

                return MidiEventFactory.MidiEvent(stream, statusByte, deltaTime);
            }
            else if (statusByte == 0xF0 || statusByte == 0xF7) // Check for SysEx events
            {
                type = EventType.SysEx;
                Console.WriteLine("SysEx event found...");
                Console.WriteLine($"Status byte: {statusByte:X}, deltaTime: {deltaTime}");

                return SysExEventFactory.SysExEvent(stream, statusByte, deltaTime);
            }
            else if (statusByte == 0xFF) // Check for meta events
            {
                type = EventType.Meta;
                Console.WriteLine("Meta event found...");
                Console.WriteLine($"DeltaTime: {deltaTime}");

                return MetaEventFactory.MetaEvent(stream, deltaTime);
            }
            else
            {
                throw new FormatException($"Unknown status byte found: {statusByte:X}");
            }
        }
    }
}
