using Beepus.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beepus
{
    public class BeepCommands
    {
        private readonly Channel[] channels = new Channel[16];
        private readonly TickDiv tickDiv;

        public BeepCommands(IEnumerable<IMidiEvent> midiEvents, IList<IMetaEvent> metaEvents, TickDiv tickDiv)
        {
            this.tickDiv = tickDiv;

            // Create the channels
            for (var i = 0; i < 16; i++)
            {
                channels[i] = new Channel();
            }

            // Set the channel names
            var instrumentNameIndicies = Enumerable.Range(0, metaEvents.Count).Where(i => metaEvents[i].Type == MetaEvent.InstrumentName && i != 0).ToArray();

            foreach (var i in instrumentNameIndicies)
            {
                if (metaEvents[i - 1].Type == MetaEvent.MIDIChannelPrefix)
                {
                    channels[((MidiChannelPrefix) metaEvents[i - 1]).Cc].Name = (metaEvents[i] as TextEvent)?.Text;
                }
            }

            // Convert the midi events to commands
            var currentNotes = new NoteUnfinished[16];

            foreach (var midiEvent in midiEvents)
            {
                for (var i = 0; i < 16; i++)
                {
                    if (currentNotes[i] != null)
                    {
                        currentNotes[i].Duration += midiEvent.DeltaTime;
                    }
                }

                switch (midiEvent.Type)
                {
                    case MidiEvent.NoteOn:
                    {
                        var note = midiEvent as Events.Note;

                        if (currentNotes[note.Channel] == null) // Check if this note can be set as current note
                        {
                            currentNotes[note.Channel] = new NoteUnfinished(note.Key);
                        }
                        else if (note.DeltaTime == 0 && note.Key > currentNotes[note.Channel].Key) // Check if this note is played at the same time as the current note and is higher
                        {
                            currentNotes[note.Channel] = new NoteUnfinished(note.Key);
                        }

                        break;
                    }
                    case MidiEvent.NoteOff:
                    {
                        var note = midiEvent as Events.Note;

                        if (currentNotes[note.Channel] != null && currentNotes[note.Channel].Key == note.Key) // Check if there is a note to finish
                        {
                            channels[note.Channel].Commands.Add(currentNotes[note.Channel].Finish());
                            currentNotes[note.Channel] = null;
                        }

                        break;
                    }
                    
                    case MidiEvent.PolyPressure:
                        break;
                    case MidiEvent.Controller:
                        break;
                    case MidiEvent.ProgramChange:
                        break;
                    case MidiEvent.ChannelPressure:
                        break;
                    case MidiEvent.PitchBend:
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            for (var i = 0; i < 16; i++)
            {
                Console.WriteLine($"Channel {i}: '{channels[i].Name}' with {channels[i].Commands.Count} commands.");
            }
        }

        public void PrintChannels()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            for (var i = 0; i < 16; i++)
            {
                Console.WriteLine($"Channel {i}: '{channels[i].Name}' with {channels[i].Commands.Count} commands.");
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public int CommandCount()
        {
            var count = 0;

            for (var i = 0; i < 16; i++)
            {
                count += channels[i].Commands.Count;
            }

            return count;
        }

        public void Beep(int channel)
        {
            channels[channel].Beep(tickDiv);
        }

        private class Channel
        {
            public string Name;
            public readonly List<Command> Commands = new List<Command>();

            public void Beep(TickDiv tickDiv)
            {
                foreach (var command in Commands)
                {
                    command.Beep(tickDiv);
                }
            }
        }

        private abstract class Command
        {
            public abstract void Beep(TickDiv tickDiv);
        }

        private class Note : Command
        {
            private readonly byte key;
            private readonly int duration;

            public Note(byte key, int duration)
            {
                this.key = key;
                this.duration = duration;
            }

            public override void Beep(TickDiv tickDiv)
            {
                Beeper.BeepMidi(key, tickDiv.GetDuration(duration));
            }
        }

        private class NoteUnfinished
        {
            public int Duration = 0;

            public byte Key { get; private set; }

            public NoteUnfinished(byte key)
            {
                Key = key;
            }

            public Command Finish()
            {
                return new Note(Key, Duration);
            }
        }
    }
}
