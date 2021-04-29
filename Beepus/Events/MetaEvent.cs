using System;
using System.Text;
using Beepus.Utils;

namespace Beepus.Events
{
    public static class MetaEventFactory
    {
        public static IEvent MetaEvent(byte[] content, int startIndex, byte statusByte, int deltaTime, out int endIndex)
        {
            byte type = content[startIndex];
            startIndex++;
            
            if (type == 0x00) // Sequence number
            {
                return new SequenceNumber(content, startIndex, deltaTime, out endIndex);
            }
            else if (type == 0x01) // Text
            {
                return new TextEvent(content, startIndex, deltaTime, Events.MetaEvent.Text, out endIndex);
            }
            else if (type == 0x02) // Copyright
            {
                return new TextEvent(content, startIndex, deltaTime, Events.MetaEvent.Copyright, out endIndex);
            }
            else if (type == 0x03) // Sequnce/Track name
            {
                return new TextEvent(content, startIndex, deltaTime, Events.MetaEvent.TrackName, out endIndex);
            }
            else if (type == 0x04) // Instrument name
            {
                return new TextEvent(content, startIndex, deltaTime, Events.MetaEvent.InstrumentName, out endIndex);
            }
            else if (type == 0x05) // Lyric
            {
                return new TextEvent(content, startIndex, deltaTime, Events.MetaEvent.Lyric, out endIndex);
            }
            else if (type == 0x06) // Marker
            {
                return new TextEvent(content, startIndex, deltaTime, Events.MetaEvent.Marker, out endIndex);
            }
            else if (type == 0x07) // Cue Point
            {
                return new TextEvent(content, startIndex, deltaTime, Events.MetaEvent.CuePoint, out endIndex);
            }
            else if (type == 0x08) // Program name
            {
                return new TextEvent(content, startIndex, deltaTime, Events.MetaEvent.ProgramName, out endIndex);
            }
            else if (type == 0x09) // Device name
            {
                return new TextEvent(content, startIndex, deltaTime, Events.MetaEvent.DeviceName, out endIndex);
            }
            else if (type == 0x20) // MIDI Channel Prefix
            {
                return new MidiChannelPrefix(content, startIndex, deltaTime, out endIndex);
            }
            else if (type == 0x21) // MIDI Port
            {
                return new MidiPort(content, startIndex, deltaTime, out endIndex);
            }
            else if (type == 0x2F) // End of Track
            {
                return new EndOfTrack(content, startIndex, deltaTime, out endIndex);
            }
            else if (type == 0x51) // Tempo
            {
                return new Tempo(content, startIndex, deltaTime, out endIndex);
            }
            else if (type == 0x54) // SMPTE offset
            {
                return new SMPTEOffset(content, startIndex, deltaTime, out endIndex);
            }
            else if (type == 0x58) // Time signature
            {
                return new TimeSignature(content, startIndex, deltaTime, out endIndex);
            }
            else if (type == 0x59) // Key signature
            {
                return new KeySignature(content, startIndex, deltaTime, out endIndex);
            }
            else if (type == 0x7F) // Sequencer signature
            {
                return new SequencerEvent(content, startIndex, deltaTime, out endIndex);
            }
            else
            {
                throw new FormatException($"The meta event type {type:X} is not supported!");
            }
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
        SequncerEvent
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

        public SequenceNumber(byte[] content, int startIndex, int deltaTime, out int endIndex)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.SequenceNumber;

            Number = ByteTools.ToUInt16BigEndian(content, ++startIndex);
            endIndex = startIndex + 2;
        }
    }

    public class TextEvent : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public string Text { get; private set; }

        public TextEvent(byte[] content, int startIndex, int deltaTime, MetaEvent type, out int endIndex)
        {
            DeltaTime = deltaTime;
            Type = type;

            int length = ByteTools.ConvertVariableLenght(content, startIndex, out int offset);
            Text = Encoding.ASCII.GetString(content, startIndex + offset, length);

            endIndex = startIndex + offset + length;
        }
    }

    public class MidiChannelPrefix : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public byte Cc { get; private set; }

        public MidiChannelPrefix(byte[] content, int startIndex, int deltaTime, out int endIndex)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.MIDIChannelPrefix;

            Cc = content[++startIndex];
            endIndex = startIndex + 1;
        }
    }

    public class MidiPort : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public byte Pp { get; private set; }

        public MidiPort(byte[] content, int startIndex, int deltaTime, out int endIndex)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.MIDIPort;

            Pp = content[++startIndex];
            endIndex = startIndex + 1;
        }
    }

    public class EndOfTrack : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public EndOfTrack(byte[] content, int startIndex, int deltaTime, out int endIndex)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.EndOfTrack;

            endIndex = startIndex + 1;
        }
    }

    public class Tempo : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public int Tt { get; private set; }

        public Tempo(byte[] content, int startIndex, int deltaTime, out int endIndex)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.Tempo;

            Tt = (int)ByteTools.ToUInt24BigEndian(content, ++startIndex);
            endIndex = startIndex + 3;
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

        public SMPTEOffset(byte[] content, int startIndex, int deltaTime, out int endIndex)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.TimeSignature;

            Hour = content[++startIndex];
            Minutes = content[++startIndex];
            Seconds = content[++startIndex];
            Frames = content[++startIndex];
            FractionalFrames = content[++startIndex];

            endIndex = startIndex + 1;
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

        public TimeSignature(byte[] content, int startIndex, int deltaTime, out int endIndex)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.TimeSignature;

            Numerator = content[++startIndex];
            Denominator = content[++startIndex];
            ClockNumber = content[++startIndex];
            QuarterNote = content[++startIndex];

            endIndex = startIndex + 1;
        }
    }

    public class KeySignature : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public byte Number { get; private set; }
        public byte Mi { get; private set; }

        public KeySignature(byte[] content, int startIndex, int deltaTime, out int endIndex)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.KeySignature;

            Number = content[++startIndex];
            Mi = content[++startIndex];

            endIndex = startIndex + 1;
        }
    }

    public class SequencerEvent : IMetaEvent
    {
        public int DeltaTime { get; private set; }
        public MetaEvent Type { get; private set; }

        public byte[] Bytes { get; private set; }

        public SequencerEvent(byte[] content, int startIndex, int deltaTime, out int endIndex)
        {
            DeltaTime = deltaTime;
            Type = MetaEvent.SequncerEvent;

            int length = ByteTools.ConvertVariableLenght(content, startIndex, out int offset);
            endIndex = startIndex + offset + length;

            Bytes = new byte[length];
            Array.Copy(content, startIndex + offset, Bytes, 0, length);
        }
    }
}
