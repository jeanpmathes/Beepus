using System;

namespace Beepus.Events
{
    public static class MidiEventFactory 
    {
        public static IEvent MidiEvent(byte[] content, int startIndex, byte statusByte, int deltaTime, out int endIndex)
        {
            byte channel = (byte) ((statusByte & 0b0000_1111) >> 4);
            byte eventType = (byte)(statusByte & 0b1111_0000);

            if (eventType == 0x80) // Note Off
            {
                endIndex = startIndex + 2;

                return new Note(deltaTime, channel, Events.MidiEvent.NoteOff, content, startIndex);
            }
            else if (eventType == 0x90) // Note On (Or Note Off if velocity is zero)
            {
                endIndex = startIndex + 2;

                return new Note(deltaTime, channel, Events.MidiEvent.NoteOn, content, startIndex);
            }
            else if (eventType == 0xA0) // Polyphonic pressure
            {
                endIndex = startIndex + 2;

                return new PolyphonicPressure(deltaTime, channel, content, startIndex);
            }
            else if (eventType == 0xB0) // Controller
            {
                endIndex = startIndex + 2;

                return new Controller(deltaTime, channel, content, startIndex);
            }
            else if (eventType == 0xC0) // Program change
            {
                endIndex = startIndex + 1;

                return new ProgramChange(deltaTime, channel, content, startIndex);
            }
            else if (eventType == 0xD0) // Channel pressure
            {
                endIndex = startIndex + 1;

                return new ChannelPressure(deltaTime, channel, content, startIndex);
            }
            else if (eventType == 0xE0) // Pitch bend
            {
                endIndex = startIndex + 2;

                return new PitchBend(deltaTime, channel, content, startIndex);
            }
            else
            {
                throw new FormatException("Unknown MidiEvent specifier!");
            }
        }
    }

    public enum MidiEvent
    {
        NoteOff,
        NoteOn,
        PolyPressure,
        Controller,
        ProgramChange,
        ChannelPressure,
        PitchBend
    }

    public interface IMidiEvent : IEvent
    {
        byte Channel { get; }
        MidiEvent Type { get; }
    }

    public class Note : IMidiEvent
    {
        public int DeltaTime { get; private set; }
        public byte Channel { get; private set; }
        public MidiEvent Type { get; private set; }

        public byte Key { get; private set; }
        public byte Velocity { get; private set; }

        public Note(int deltaTime, byte channel, MidiEvent type, byte[] content, int startIndex)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = type;

            Key = content[startIndex];
            Velocity = content[startIndex + 1];

            if (Velocity == 0)
            {
                Type = MidiEvent.NoteOff;
            }
        }
    }

    public class PolyphonicPressure : IMidiEvent
    {
        public int DeltaTime { get; private set; }
        public byte Channel { get; private set; }
        public MidiEvent Type { get; private set; }

        public byte Key { get; private set; }
        public byte Pressure { get; private set; }

        public PolyphonicPressure(int deltaTime, byte channel, byte[] content, int startIndex)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.PolyPressure;

            Key = content[startIndex];
            Pressure = content[startIndex + 1];
        }
    }

    public class Controller : IMidiEvent
    {
        public int DeltaTime { get; private set; }
        public byte Channel { get; private set; }
        public MidiEvent Type { get; private set; }

        public byte ControllerNumber { get; private set; }
        public byte Value { get; private set; }

        public Controller(int deltaTime, byte channel, byte[] content, int startIndex)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.Controller;

            ControllerNumber = content[startIndex];
            Value = content[startIndex + 1];
        }
    }

    public class ProgramChange : IMidiEvent
    {
        public int DeltaTime { get; private set; }
        public byte Channel { get; private set; }
        public MidiEvent Type { get; private set; }

        public byte Program { get; private set; }

        public ProgramChange(int deltaTime, byte channel, byte[] content, int startIndex)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.ProgramChange;

            Program = content[startIndex];
        }
    }

    public class ChannelPressure : IMidiEvent
    {
        public int DeltaTime { get; private set; }
        public byte Channel { get; private set; }
        public MidiEvent Type { get; private set; }

        public byte pressure { get; private set; }

        public ChannelPressure(int deltaTime, byte channel, byte[] content, int startIndex)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.ChannelPressure;

            pressure = content[startIndex];
        }
    }

    public class PitchBend : IMidiEvent
    {
        public int DeltaTime { get; private set; }
        public byte Channel { get; private set; }
        public MidiEvent Type { get; private set; }

        public byte Lsb { get; private set; }
        public byte Msb { get; private set; }

        public PitchBend(int deltaTime, byte channel, byte[] content, int startIndex)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.PitchBend;

            Lsb = content[startIndex];
            Msb = content[startIndex + 1];
        }
    }
}
