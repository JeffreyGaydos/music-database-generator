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

        List<char> seperators = new List<char> { ',', '/', '&', ';' };
        private List<string> GetIndividualPersons(string personString)
        {
            //this situation means that the person string is likely in the form "Last, First M." form, so the comma should not be used as a separater
            if(personString.Count(c => c == ',') == 1 && personString.Count(c => c != ',' && seperators.Contains(c)) == 0) {
                return new List<string> { personString };
            }

            List<string> result = new List<string> { personString };

            foreach(char sep in seperators)
            {
                result = result.SelectMany(s => s.Split(sep)).ToList();
            }

            return result.Select(s => s.Trim()).Distinct().ToList();
        }

        public void Generate()
        {
            foreach(string person in _file.Tag.Composers)
            {
                foreach(string individualPerson in GetIndividualPersons(person))
                {
                    if (!string.IsNullOrWhiteSpace(individualPerson))
                    {
                        _data.artistPersons.Add(new ArtistPersons
                        {
                            PersonName = individualPerson,
                            PermanentMember = true
                        });
                        _data.trackPersons.Add((new TrackPersons(), individualPerson));
                    }
                }
            }
            if(_data.artistPersons.Count == 0)
            {
                foreach (string person in _file.Tag.Performers)
                {
                    foreach (string individualPerson in GetIndividualPersons(person))
                    {
                        if (!string.IsNullOrWhiteSpace(individualPerson))
                        {
                            _data.artistPersons.Add(new ArtistPersons
                            {
                                PersonName = individualPerson,
                                PermanentMember = true
                            });
                            _data.trackPersons.Add((new TrackPersons(), individualPerson));
                        }
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
