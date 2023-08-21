using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator.Generators
{
    public class ArtistPersonGenerator : AGenerator, IGenerator
    {
        public ArtistPersonGenerator(TagLib.File file, MusicLibraryTrack data, LoggingUtils logger)
        {
            _file = file;
            _data = data;
            _logger = logger;
        }

        public void Generate()
        {
            foreach(string person in _file.Tag.Composers)
            {
                if(string.IsNullOrWhiteSpace(person))
                {
                    _data.artistPersons.Add(new ArtistPersons
                    {
                        PersonName = person,
                    });
                }
            }
            if(_data.artistPersons.Count == 0)
            {
                foreach (string person in _file.Tag.Performers)
                {
                    if (string.IsNullOrWhiteSpace(person))
                    {
                        _data.artistPersons.Add(new ArtistPersons
                        {
                            PersonName = person,
                        });
                    }
                }
            }
            if(_data.artistPersons.Count == 0)
            {
                //we don't really care if we can't find a primary person. Not all artists reveal this information
                _logger.GenerationLogWriteData($"Artist Person: No personal data found for track  \"{_data.main.Title}\"...", true);
                return;
            } else
            {
                _logger.GenerationLogWriteData($"Artist Person: \"Composers\" metadata did not exist on track \"{_data.main.Title}\". Using \"Performers\" data instead...", true);
                return;
            }
        }
    }
}
