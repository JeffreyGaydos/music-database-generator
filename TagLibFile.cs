using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator
{
    public class TagLibFile
    {
        public string path;
        public TagLib.File mp3;

        public TagLibFile(TagLib.File mp3, string path) {
            this.mp3 = mp3;
            this.path = path;
        }
    }
}
