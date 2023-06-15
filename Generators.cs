using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Matroska;

namespace MusicDatabaseGenerator
{
    public static class Generators
    {
        public Main MainFields(List<TagLib.File> tracks)
        {
            foreach(TagLib.Tag track in tracks.Select(t => t.Tag))
            {
                Console.log(track.Title);
            }
        }
    }
}
