using Beepus.Events;
using Beepus.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace Beepus
{
    public class TrackChunk
    {
        public uint TrackLenght { get; private set; }
        public string TrackName { get; private set; } = "no name";
        public IEvent[] events { get; private set; }

        public List<IMidiEvent> midiEvents { get; private set; } = new List<IMidiEvent>();
        public List<IMetaEvent> metaEvents { get; private set; } = new List<IMetaEvent>();

        public TrackChunk(FileStream stream)
        {
            if (ByteTools.CompareContent(stream, 0x4D, 0x54, 0x72, 0x6B)) // Check for "MTrk" identifier
            {
                TrackLenght = ByteTools.ReadUInt32BigEndian(stream);

                long startPosition = stream.Position;
                
                var events = new List<IEvent>();
                byte lastStatus = 0x00;

                while (stream.Position - startPosition < TrackLenght) // Reading all events
                {
                    IEvent newEvent = EventReader.ReadEvent(stream, out EventType type, out byte newStatus, lastStatus);

                    lastStatus = newStatus;

                    events.Add(newEvent);

                    if (type == EventType.Midi)
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
