using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MusicDatabaseGenerator
{
    public static class Generators
    {
        public static List<Main> MainFields(List<TagLib.File> tracks, List<string> filenames)
        {
            List<Main> mains = new List<Main>();
            for(int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                mains.Add(new Main()
                {
                    Title = track.Tag.Title,
                    FilePath = Path.GetFullPath(filenames[i]),
                    Duration = (decimal)track.Properties.Duration.TotalSeconds, //NOTE: Overflow exception possible, but unlikely
                });
            }
            return mains;
        }
    }
}
