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
        
        private readonly FileStream stream;
        private TrackChunk[] tracks;

        public MidiFile(string path)
        {
            Console.WriteLine($"Attempting to open the file at: {path}");
            
            stream = File.OpenRead(path);
            Initialise();
        }

        private void Initialise()
        {
            Console.WriteLine("Reading file...");

            // Reading the header chunk
            Console.WriteLine("Reading header...");

            if (!ByteTools.CompareContent(stream, 0x4D, 0x54, 0x68, 0x64)) // Check for "MThd" identifier
            {
                throw new FormatException("File does not begin with MThd identifier");
            }

            uint headerLength = ByteTools.ReadUInt32BigEndian(stream);
            Console.WriteLine($"Header chunk length: {headerLength}");

            if (headerLength != 6) // Check if the header chunk has the size 6 bytes
            {
                throw new FormatException("Header chunk lenght is not 6");
            }

            FormatType = ByteTools.ReadUInt16BigEndian(stream); // Set the format type (possible types are 0, 1, and 2)
            Console.WriteLine($"Format type: {FormatType}");

            if (FormatType != 1)
            {
                throw new NotImplementedException("Only format type 1 is currently implemented");
            }

            NTracks = ByteTools.ReadUInt16BigEndian(stream); // Get the amount of MTrk chunks
            Console.WriteLine($"The file has {NTracks} MTrk chunks");
            tracks = new TrackChunk[NTracks];

            TickDiv = new TickDiv((byte)stream.ReadByte(), (byte)stream.ReadByte()); // Set the tick div

            // Reading the track chunks
            Console.WriteLine("Reading track chunks...");

            for (var i = 0; i < NTracks; i++)
            {
                tracks[i] = new TrackChunk(stream);

                Console.WriteLine($"Found a track chunk with a lenght of {tracks[i].TrackLenght}");
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
