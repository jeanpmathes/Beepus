using System;

using Beepus.Events;

namespace Beepus
{
    public static class Beeper
    {
        private static int _bpm = 120;

        public static int Bpm
        {
            get => _bpm;

            set
            {
                _bpm = value;
                _wholeNoteDuration = 60000 / value * 4;
            }
        }

        private static int _wholeNoteDuration = 60000 / 120 * 4;

        public static void Beep(Note note, NoteValue value, int octave = 0)
        {
            int frequency = GetFrequency(note, octave);
            int duration = ValueToDuration(value);

            Console.Beep(frequency, duration);
        }

        private static int ValueToDuration(NoteValue value)
        {
            return _wholeNoteDuration / (int)value;
        }

        private static int GetFrequency(Note note, int octave = 0)
        {
            octave += 3;
            int number = octave * 12 + (int)note + 4;
            double frequency = 440 * Math.Pow(2d, (number - 49d) / 12d);

            return (int)frequency;
        }

        public static void BeepMidi(int key, int microseonds)
        {
            var frequency = (int)(Math.Pow(2d, (key - 69) / 12d) * 440);
            Console.Beep(frequency, microseonds / 1000);
        }

        public static BeepCommands[] GetBeepCommands(MidiFile file)
        {
            var beeps = new BeepCommands[file.NTracks];

            for (int i = 0; i < file.NTracks; i++)
            {
                beeps[i] = new BeepCommands(file.GetTrack(i).midiEvents, file.GetTrack(i).metaEvents, file.TickDiv);
            }

            return beeps;
        }
    }

    public enum NoteValue
    {
        Whole = 1,
        Half = 2,
        Quarter = 4,
        Eigth = 8,
        Sixteenth = 16,
        ThirtySecond = 32,
        SixtyFour = 64
    }

    public enum Note
    {
        C,
        CSharp,
        D,
        DSharp,
        E,
        F,
        FSharp,
        G,
        AFlat,
        A,
        ASharp,
        B
    }
}
