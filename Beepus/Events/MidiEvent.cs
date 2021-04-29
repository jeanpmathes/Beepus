using System;
using System.IO;

namespace Beepus.Events
{
    public static class MidiEventFactory 
    {
        public static IEvent MidiEvent(FileStream stream, byte statusByte, int deltaTime)
        {
            byte channel = (byte) ((statusByte & 0b0000_1111) >> 4);
            byte eventType = (byte)(statusByte & 0b1111_0000);

            if (eventType == 0x80) // Note Off
            {
                return new Note(deltaTime, channel, Events.MidiEvent.NoteOff, stream);
            }
            else if (eventType == 0x90) // Note On (Or Note Off if velocity is zero)
            {
                return new Note(deltaTime, channel, Events.MidiEvent.NoteOn, stream);
            }
            else if (eventType == 0xA0) // Polyphonic pressure
            {
                return new PolyphonicPressure(deltaTime, channel, stream);
            }
            else if (eventType == 0xB0) // Controller
            {
                return new Controller(deltaTime, channel, stream);
            }
            else if (eventType == 0xC0) // Program change
            {
                return new ProgramChange(deltaTime, channel, stream);
            }
            else if (eventType == 0xD0) // Channel pressure
            {
                return new ChannelPressure(deltaTime, channel, stream);
            }
            else if (eventType == 0xE0) // Pitch bend
            {
                return new PitchBend(deltaTime, channel, stream);
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

        public Note(int deltaTime, byte channel, MidiEvent type, FileStream stream)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = type;

            Key = (byte) stream.ReadByte();
            Velocity = (byte) stream.ReadByte();

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

        public PolyphonicPressure(int deltaTime, byte channel, FileStream stream)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.PolyPressure;

            Key = (byte) stream.ReadByte();
            Pressure = (byte) stream.ReadByte();
        }
    }

    public class Controller : IMidiEvent
    {
        public int DeltaTime { get; private set; }
        public byte Channel { get; private set; }
        public MidiEvent Type { get; private set; }

        public byte ControllerNumber { get; private set; }
        public byte Value { get; private set; }

        public Controller(int deltaTime, byte channel, FileStream stream)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.Controller;

            ControllerNumber = (byte) stream.ReadByte();
            Value = (byte) stream.ReadByte();
        }
    }

    public class ProgramChange : IMidiEvent
    {
        public int DeltaTime { get; private set; }
        public byte Channel { get; private set; }
        public MidiEvent Type { get; private set; }

        public byte Program { get; private set; }

        public ProgramChange(int deltaTime, byte channel, FileStream stream)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.ProgramChange;

            Program = (byte) stream.ReadByte();
        }
    }

    public class ChannelPressure : IMidiEvent
    {
        public int DeltaTime { get; private set; }
        public byte Channel { get; private set; }
        public MidiEvent Type { get; private set; }

        public byte pressure { get; private set; }

        public ChannelPressure(int deltaTime, byte channel, FileStream stream)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.ChannelPressure;

            pressure = (byte) stream.ReadByte();
        }
    }

    public class PitchBend : IMidiEvent
    {
        public int DeltaTime { get; private set; }
        public byte Channel { get; private set; }
        public MidiEvent Type { get; private set; }

        public byte Lsb { get; private set; }
        public byte Msb { get; private set; }

        public PitchBend(int deltaTime, byte channel, FileStream stream)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.PitchBend;

            Lsb = (byte) stream.ReadByte();
            Msb = (byte) stream.ReadByte();
        }
    }
}
