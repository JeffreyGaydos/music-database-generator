using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator.Generators
{
    public class ArtistPersonGenerator : AGenerator, IGenerator
    {
        public ArtistPersonGenerator(TagLib.File file, MusicLibraryTrack data)
        {
            _file = file;
            _data = data;
        }

        public void Generate()
        {
            foreach(string person in _file.Tag.Composers)
            {
                _data.artistPersons.Add(new ArtistPersons
                {
                    PersonName = person,
                });
            }
        }
    }
}
