using Beepus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beepus
{
    public class BeepCommands
    {
        private Channel[] channels = new Channel[16];
        private TickDiv tickDiv;

        public BeepCommands(List<IMidiEvent> midiEvents, List<IMetaEvent> metaEvents, TickDiv tickDiv)
        {
            this.tickDiv = tickDiv;

            // Create the channels
            for (int i = 0; i < 16; i++)
            {
                channels[i] = new Channel();
            }

            // Set the channel names
            int[] instrumentNameIndicies = Enumerable.Range(0, metaEvents.Count).Where(i => metaEvents[i].Type == MetaEvent.InstrumentName && i != 0).ToArray();

            foreach (int i in instrumentNameIndicies)
            {
                if (metaEvents[i - 1].Type == MetaEvent.MIDIChannelPrefix)
                {
                    channels[(metaEvents[i - 1] as MidiChannelPrefix).Cc].name = (metaEvents[i] as TextEvent).Text;
                }
            }

            // Convert the midi events to commands
            NoteUnfinished[] currentNotes = new NoteUnfinished[16];

            foreach (IMidiEvent midiEvent in midiEvents)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (currentNotes[i] != null)
                    {
                        currentNotes[i].duration += midiEvent.DeltaTime;
                    }
                }

                if (midiEvent.Type == MidiEvent.NoteOn)
                {
                    Events.Note note = midiEvent as Events.Note;

                    if (currentNotes[note.Channel] == null) // Check if this note can be set as current note
                    {
                        currentNotes[note.Channel] = new NoteUnfinished(note.Key);
                    }
                    else if (note.DeltaTime == 0 && note.Key > currentNotes[note.Channel].Key) // Check if this note is played at the same time as the current note and is higher
                    {
                        currentNotes[note.Channel] = new NoteUnfinished(note.Key);
                    }
                }

                if (midiEvent.Type == MidiEvent.NoteOff)
                {
                    Events.Note note = midiEvent as Events.Note;

                    if (currentNotes[note.Channel] != null && currentNotes[note.Channel].Key == note.Key) // Check if there is a note to finish
                    {
                        channels[note.Channel].commands.Add(currentNotes[note.Channel].Finish());
                        currentNotes[note.Channel] = null;
                    }
                }
            }

            for (int i = 0; i < 16; i++)
            {
                Console.WriteLine($"Channel {i}: '{channels[i].name}' with {channels[i].commands.Count} commands.");
            }
        }

        public void PrintChannels()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            for (int i = 0; i < 16; i++)
            {
                Console.WriteLine($"Channel {i}: '{channels[i].name}' with {channels[i].commands.Count} commands.");
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public int CommandCount()
        {
            int count = 0;

            for (int i = 0; i < 16; i++)
            {
                count += channels[i].commands.Count;
            }

            return count;
        }

        public void Beep(int channel)
        {
            channels[channel].Beep(tickDiv);
        }

        private class Channel
        {
            public string name;
            public List<Command> commands = new List<Command>();

            public void Beep(TickDiv tickDiv)
            {
                foreach (Command command in commands)
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
            private byte key;
            private int duration;

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
            public int duration = 0;

            public byte Key { get; private set; }

            public NoteUnfinished(byte key)
            {
                Key = key;
            }

            public Command Finish()
            {
                return new Note(Key, duration);
            }
        }
    }
}
