using System;
using System.IO;

namespace Beepus.Events
{
    public static class MidiEventFactory 
    {
        public static IEvent MidiEvent(Stream stream, byte statusByte, int deltaTime)
        {
            var channel = (byte) ((statusByte & 0b0000_1111) >> 4);
            var eventType = (byte)(statusByte & 0b1111_0000);

            return eventType switch
            {
                // Note Off
                0x80 => new Note(deltaTime, channel, Events.MidiEvent.NoteOff, stream),
                // Note On (Or Note Off if velocity is zero)
                0x90 => new Note(deltaTime, channel, Events.MidiEvent.NoteOn, stream),
                // Polyphonic pressure
                0xA0 => new PolyphonicPressure(deltaTime, channel, stream),
                // Controller
                0xB0 => new Controller(deltaTime, channel, stream),
                // Program change
                0xC0 => new ProgramChange(deltaTime, channel, stream),
                // Channel pressure
                0xD0 => new ChannelPressure(deltaTime, channel, stream),
                // Pitch bend
                0xE0 => new PitchBend(deltaTime, channel, stream),
                _ => throw new FormatException("Unknown MidiEvent specifier!")
            };
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
        public int DeltaTime { get; }
        public byte Channel { get; }
        public MidiEvent Type { get; }

        public byte Key { get; }
        public byte Velocity { get; }

        public Note(int deltaTime, byte channel, MidiEvent type, Stream stream)
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
        public int DeltaTime { get; }
        public byte Channel { get; }
        public MidiEvent Type { get; }

        public byte Key { get; }
        public byte Pressure { get; }

        public PolyphonicPressure(int deltaTime, byte channel, Stream stream)
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
        public int DeltaTime { get; }
        public byte Channel { get; }
        public MidiEvent Type { get; }

        public byte ControllerNumber { get; }
        public byte Value { get; }

        public Controller(int deltaTime, byte channel, Stream stream)
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
        public int DeltaTime { get; }
        public byte Channel { get; }
        public MidiEvent Type { get; }

        public byte Program { get; }

        public ProgramChange(int deltaTime, byte channel, Stream stream)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.ProgramChange;

            Program = (byte) stream.ReadByte();
        }
    }

    public class ChannelPressure : IMidiEvent
    {
        public int DeltaTime { get; }
        public byte Channel { get; }
        public MidiEvent Type { get; }

        public byte Pressure { get; }

        public ChannelPressure(int deltaTime, byte channel, Stream stream)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.ChannelPressure;

            Pressure = (byte) stream.ReadByte();
        }
    }

    public class PitchBend : IMidiEvent
    {
        public int DeltaTime { get; }
        public byte Channel { get; }
        public MidiEvent Type { get; }

        public byte Lsb { get; }
        public byte Msb { get; }

        public PitchBend(int deltaTime, byte channel, Stream stream)
        {
            DeltaTime = deltaTime;
            Channel = channel;
            Type = MidiEvent.PitchBend;

            Lsb = (byte) stream.ReadByte();
            Msb = (byte) stream.ReadByte();
        }
    }
}
