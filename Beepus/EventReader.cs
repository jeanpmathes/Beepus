using Beepus.Events;
using System;
using System.Collections.Generic;
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
        public static IEvent ReadEvent(byte[] content, int startIndex, out int endIndex, out EventType type, out byte statusByte, byte lastStatus = 0x00)
        {
            int deltaTime = ByteTools.ConvertVariableLenght(content, startIndex, out int lenght);
            startIndex += lenght;

            statusByte = content[startIndex];

            if (statusByte < 0x80) // Check for running status
            {
                if (lastStatus == 0x00)
                {
                    throw new FormatException($"Running status found but no correct status! Index: {startIndex}");
                }

                statusByte = lastStatus;
            }
            else
            {
                startIndex++;
            }

            if (statusByte >= 0x80 && statusByte <= 0xEF) // Check for MIDI events
            {
                type = EventType.Midi;
                Console.WriteLine("MIDI event found...");
                Console.WriteLine($"Status byte: {statusByte:X}, deltaTime: {deltaTime}, position: {startIndex - 1}");

                return MidiEventFactory.MidiEvent(content, startIndex, statusByte, deltaTime, out endIndex);
            }
            else if (statusByte == 0xF0 || statusByte == 0xF7) // Check for SysEx events
            {
                type = EventType.SysEx;
                Console.WriteLine("SysEx event found...");
                Console.WriteLine($"Status byte: {statusByte:X}, deltaTime: {deltaTime}, position: {startIndex - 1}");

                return SysExEventFactory.SysExEvent(content, startIndex, statusByte, deltaTime, out endIndex);
            }
            else if (statusByte == 0xFF) // Check for meta events
            {
                type = EventType.Meta;
                Console.WriteLine("Meta event found...");
                Console.WriteLine($"Status byte: {statusByte:X}, deltaTime: {deltaTime}, position: {startIndex - 1}");

                return MetaEventFactory.MetaEvent(content, startIndex, statusByte, deltaTime, out endIndex);
            }
            else
            {
                throw new FormatException($"Unknown status byte found: {statusByte:X}");
            }
        }
    }
}
