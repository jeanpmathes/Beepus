using System;

namespace Beepus
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length != 1) return;
            
            var midiFile = new MidiFile(args[0]);

            BeepCommands[] beeps = Beeper.GetBeepCommands(midiFile);

            // Get user input to choose what to beep
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.WriteLine("#########################################");
            Console.WriteLine("The following tracks are available:");
            midiFile.PrintTracks(beeps);
            
            if (!int.TryParse(Console.ReadLine(), out int track))
            {
                track = 1;
            }

            Console.WriteLine("The following channels are available in this track:");
            beeps[track].PrintChannels();
            
            if (!int.TryParse(Console.ReadLine(), out int channel))
            {
                channel = 0;
            }

            Console.WriteLine("Playing...");
            beeps[track].Beep(channel);

            Console.ReadKey();
        }
    }
}
