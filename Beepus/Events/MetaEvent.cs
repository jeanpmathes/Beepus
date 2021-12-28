using System;
using System.IO;
using System.Text;
using Beepus.Utils;

namespace Beepus.Events
{
    public static class MetaEventFactory
    {
        public static IEvent MetaEvent(Stream stream, int deltaTime)
        {
            var type = (byte) stream.ReadByte();
            var lenght = ByteTools.ReadVariableLenght(stream);

            return type switch
            {
                0x00 => // Sequence number
                    new SequenceNumber(stream, deltaTime),
                0x01 => // Text
                    new TextEvent(stream, lenght, deltaTime, Events.MetaEvent.Text),
                0x02 => // Copyright
                    new TextEvent(stream, lenght, deltaTime, Events.MetaEvent.Copyright),
                0x03 => // Sequence/Track name
                    new TextEvent(stream, lenght, deltaTime, Events.MetaEvent.TrackName),
                0x04 => // Instrument name
                    new TextEvent(stream, lenght, deltaTime, Events.MetaEvent.InstrumentName),
                0x05 => // Lyric
                    new TextEvent(stream, lenght, deltaTime, Events.MetaEvent.Lyric),
                0x06 => // Marker
                    new TextEvent(stream, lenght, deltaTime, Events.MetaEvent.Marker),
                0x07 => // Cue Point
                    new TextEvent(stream, lenght, deltaTime, Events.MetaEvent.CuePoint),
                0x08 => // Program name
                    new TextEvent(stream, lenght, deltaTime, Events.MetaEvent.ProgramName),
                0x09 => // Device name
                    new TextEvent(stream, lenght, deltaTime, Events.MetaEvent.DeviceName),
                0x20 => // MIDI Channel Prefix
                    new MidiChannelPrefix(stream, deltaTime),
                0x21 => // MIDI Port
                    new MidiPort(stream, deltaTime),
                0x2F => // End of Track
                    new EndOfTrack(deltaTime),
                0x51 => // Tempo
                    new Tempo(stream, deltaTime),
                0x54 => // SMPTE offset
                    new SMPTEOffset(stream, deltaTime),
                0x58 => // Time signature
                    new TimeSignature(stream, deltaTime),
                0x59 => // Key signature
                    new KeySignature(stream, deltaTime),
                0x7F => // Sequencer signature
                    new SequencerEvent(stream, lenght, deltaTime),
                _ => throw new FormatException($"The meta event type {type:X} is not supported!")
            };
        }
    }

    public enum MetaEvent
    {
        SequenceNumber,
        Text,
        Copyright,
        TrackName,
        InstrumentName,
        Lyric,
        Marker,
        CuePoint,
        ProgramName,
        DeviceName,
        MIDIChannelPrefix,
        MIDIPort,
        EndOfTrack,
        Tempo,
        SMPTE,
        TimeSignature,
        KeySignature,
        SequencerEvent
    }

    public interface IMetaEvent : IEvent
    {
        MetaEvent Type { get; }
    }

    public class SequenceNumber : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public ushort Number { get; private set; }

        public SequenceNumber(Stream stream, int deltaTime)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.SequenceNumber;

            Number = ByteTools.ReadUInt16BigEndian(stream);
        }
    }

    public class TextEvent : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public string Text { get; private set; }

        public TextEvent(Stream stream, int length, int deltaTime, MetaEvent type)
        {
            DeltaTime = deltaTime;
            Type = type;

            var buffer = new byte[length];
            stream.Read(buffer, 0, length);
            
            Text = Encoding.ASCII.GetString(buffer, 0, length);
        }
    }

    public class MidiChannelPrefix : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public byte Cc { get; private set; }

        public MidiChannelPrefix(Stream stream, int deltaTime)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.MIDIChannelPrefix;

            Cc = (byte) stream.ReadByte();
        }
    }

    public class MidiPort : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public byte Pp { get; private set; }

        public MidiPort(Stream stream, int deltaTime)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.MIDIPort;

            Pp = (byte) stream.ReadByte();
        }
    }

    public class EndOfTrack : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public EndOfTrack(int deltaTime)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.EndOfTrack;
        }
    }

    public class Tempo : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public int Tt { get; private set; }

        public Tempo(Stream stream, int deltaTime)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.Tempo;

            Tt = (int)ByteTools.ReadUInt24BigEndian(stream);
        }
    }

    public class SMPTEOffset : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public byte Hour { get; private set; }
        public byte Minutes { get; private set; }
        public byte Seconds { get; private set; }
        public byte Frames { get; private set; }
        public byte FractionalFrames { get; private set; }

        public SMPTEOffset(Stream stream, int deltaTime)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.TimeSignature;

            Hour = (byte) stream.ReadByte();
            Minutes =(byte) stream.ReadByte();
            Seconds = (byte) stream.ReadByte();
            Frames = (byte) stream.ReadByte();
            FractionalFrames = (byte) stream.ReadByte();
        }
    }

    public class TimeSignature : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public byte Numerator { get; private set; }
        public byte Denominator { get; private set; }
        public byte ClockNumber { get; private set; }
        public byte QuarterNote { get; private set; }

        public TimeSignature(Stream stream, int deltaTime)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.TimeSignature;

            Numerator = (byte) stream.ReadByte();
            Denominator = (byte) stream.ReadByte();
            ClockNumber = (byte) stream.ReadByte();
            QuarterNote = (byte) stream.ReadByte();
        }
    }

    public class KeySignature : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public byte Number { get; private set; }
        public byte Mi { get; private set; }

        public KeySignature(Stream stream, int deltaTime)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.KeySignature;

            Number = (byte) stream.ReadByte();
            Mi = (byte) stream.ReadByte();
        }
    }

    public class SequencerEvent : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public byte[] Bytes { get; private set; }

        public SequencerEvent(Stream stream, int length, int deltaTime)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.SequencerEvent;

            Bytes = new byte[length];
            stream.Read(Bytes, 0, length);
        }
    }
}
