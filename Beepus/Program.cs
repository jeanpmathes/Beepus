using System;
using System.Windows.Forms;

namespace Beepus
{   
    class Program
    {
        [STAThread]
        static void Main()
        {
            MidiFile midiFile = new MidiFile(GetPath());

            BeepCommands[] beeps = Beeper.GetBeepCommands(midiFile);

            // Get user input to choose what to beep
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.WriteLine("#########################################");
            Console.WriteLine("The following tracks are available:");
            midiFile.PrintTracks(beeps);
            if (int.TryParse(Console.ReadLine(), out int track))
            {

            }
            else
            {
                track = 1;
            }

            Console.WriteLine("The following channels are available in this track:");
            beeps[track].PrintChannels();
            if (int.TryParse(Console.ReadLine(), out int channel))
            {

            }
            else
            {
                channel = 0;
            }

            Console.WriteLine("Playing...");
            beeps[track].Beep(channel);

            Console.ReadKey();
        }

        public static string GetPath()
        {
            string path = "";

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "MIDI files (*.mid)|*.mid";
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    path = dialog.FileName;
                }
            }

            return path;
        }
    }
}
