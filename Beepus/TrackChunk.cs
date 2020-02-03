using Beepus.Events;
using Beepus.Utils;
using System;
using System.Collections.Generic;

namespace Beepus
{
    public class TrackChunk
    {
        public uint TrackLenght { get; private set; }
        public string TrackName { get; private set; } = "no name";
        public IEvent[] events { get; private set; }

        public List<IMidiEvent> midiEvents { get; private set; } = new List<IMidiEvent>();
        public List<IMetaEvent> metaEvents { get; private set; } = new List<IMetaEvent>();

        public TrackChunk(byte[] content, int startIndex)
        {
            if (ByteTools.CompareContent(content, startIndex, 0x4D, 0x54, 0x72, 0x6B)) // Check for "MTrk" identifier
            {
                TrackLenght = ByteTools.ToUInt32BigEndian(content, startIndex + 4);

                int readerIndex = startIndex + 8;
                List<IEvent> events = new List<IEvent>();
                byte lastStatus = 0x00;

                while (readerIndex < TrackLenght + startIndex) // Reading all events
                {
                    IEvent newEvent = EventReader.ReadEvent(content, readerIndex, out int endIndex, out EventType type, out byte newStatus, lastStatus);

                    lastStatus = newStatus;
                    readerIndex = endIndex;

                    events.Add(newEvent);

                    if (type == EventType.MIDI)
                    {
                        midiEvents.Add(newEvent as IMidiEvent);
                    }

                    if (type == EventType.Meta)
                    {
                        IMetaEvent metaEvent = newEvent as IMetaEvent;

                        if (metaEvent.Type == MetaEvent.TrackName)
                        {
                            TrackName = (metaEvent as TextEvent).Text;
                        }

                        metaEvents.Add(metaEvent);
                    }
                }

                Console.WriteLine("Reached the end of this track!");
                this.events = events.ToArray();
            }
            else
            {
                throw new FormatException("No MTrk identifier found");
            }
        }
    }
}
