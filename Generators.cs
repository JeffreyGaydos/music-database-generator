using System;
using System.Collections.Generic;
using System.IO;

namespace MusicDatabaseGenerator
{
    public static class Generators
    {
        public static List<Main> MainFields(List<TagLib.File> tracks, List<string> filenames)
        {
            List<Main> main = new List<Main>();
            for(int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                var file = filenames[i];
                main.Add(new Main()
                {
                    Title = track.Tag.Title,
                    FilePath = Path.GetFullPath(file),
                    Duration = (decimal)track.Properties.Duration.TotalSeconds, //NOTE: Overflow exception possible, but unlikely
                    ReleaseYear = (int?)track.Tag.Year == 0 ? null : (int?)track.Tag.Year, //NOTE: Overflow exception possible, but unlikely
                    AddDate = new FileInfo(file).LastWriteTime > new FileInfo(file).CreationTime ? new FileInfo(file).CreationTime : new FileInfo(file).LastWriteTime
                });
                Console.WriteLine($"Finished processing track {i} of {tracks.Count} ({track.Tag.Title})");
            }
            return main;
        }
    }
}
