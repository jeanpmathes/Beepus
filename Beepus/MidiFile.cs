using System;
using System.IO;
using Beepus.Events;
using Beepus.Utils;

namespace Beepus
{
    public class MidiFile
    {
        private ushort FormatType { get; set; }
        public ushort NTracks { get; private set; }
        public TickDiv TickDiv { get; private set; }

        private readonly byte[] content;
        private TrackChunk[] tracks;

        public MidiFile(string path)
        {
            Console.WriteLine($"Attempting to open the file at: {path}");

            content = File.ReadAllBytes(path);
            Initialise();
        }

        private void Initialise()
        {
            Console.WriteLine("Reading file...");

            // Reading the header chunk
            Console.WriteLine("Reading header...");

            if (!ByteTools.CompareContent(content, 0, 0x4D, 0x54, 0x68, 0x64)) // Check for "MThd" identifier
            {
                throw new FormatException("File does not begin with MThd identifier");
            }

            uint headerLength = ByteTools.ToUInt32BigEndian(content, 4);
            Console.WriteLine($"Header chunk length: {headerLength}");

            if (headerLength != 6) // Check if the header chunk has the size 6 bytes
            {
                throw new FormatException("Header chunk lenght is not 6");
            }

            FormatType = ByteTools.ToUInt16BigEndian(content, 8); // Set the format type (possible types are 0, 1, and 2)
            Console.WriteLine($"Format type: {FormatType}");

            if (FormatType != 1)
            {
                throw new NotImplementedException("Only format type 1 is currently implemented");
            }

            NTracks = ByteTools.ToUInt16BigEndian(content, 10); // Get the amount of MTrk chunks
            Console.WriteLine($"The file has {NTracks} MTrk chunks");
            tracks = new TrackChunk[NTracks];

            TickDiv = new TickDiv(content[12], content[13]); // Set the tick div

            // Reading the track chunks
            Console.WriteLine("Reading track chunks...");
            int currentPosition = 8 + (int) headerLength;

            for (var i = 0; i < NTracks; i++)
            {
                if (ByteTools.CompareContent(content, currentPosition, 0x4D, 0x54, 0x72, 0x6B)) // Check for "MTrk" identifier
                {
                    tracks[i] = new TrackChunk(content, currentPosition);

                    Console.WriteLine($"Found a track chunk at {currentPosition} with a lenght of {tracks[i].TrackLenght}");
                    currentPosition += (int)tracks[i].TrackLenght + 8;
                }
                else
                {
                    throw new FormatException($"MTrk idetifier not found at byte {currentPosition}");
                }
            }

            // Setting the tempo (Only works for metrical timing and format 1)
            TickDiv.MicrosecondsPerQuarterNote = ((Tempo) tracks[0].metaEvents.Find(e => e.Type == Events.MetaEvent.Tempo)).Tt;
        }

        public void PrintTracks(BeepCommands[] commands)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            for (var i = 1; i < tracks.Length; i++)
            {
                Console.WriteLine($"Track {i}: {tracks[i].TrackName} with a total of {commands[i].CommandCount()} commands");
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public TrackChunk GetTrack(int index)
        {
            return tracks[index];
        }
    }
}
