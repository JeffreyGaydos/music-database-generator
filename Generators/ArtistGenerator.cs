using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MusicDatabaseGenerator.Generators
{
    public class ArtistGenerator : AGenerator, IGenerator
    {
        private Regex artistNameReg = new Regex("(?<=[\\\\\\/])[^\\\\\\/]*(?= - [^\\\\\\/]* - [^\\\\\\/]*\\.[a-zA-Z]+)", RegexOptions.Compiled);
        private Regex folderArtistNameReg = new Regex("(?<=[\\\\\\/])[^\\\\\\/]+(?=[\\\\\\/][^\\\\\\/]+[\\\\\\/][^\\\\\\/]+\\.)", RegexOptions.Compiled);
        private string _inputPath;
        public ArtistGenerator(TagLib.File file, MusicLibraryTrack data, string inputPath, LoggingUtils logger)
        {
            _file = file;
            _data = data;
            _inputPath = inputPath;
            _logger = logger;
        }

        public void Generate()
        {
            string pathData = _file.Name.IndexOf(_inputPath) == -1 ? _file.Name : _file.Name.Substring(_file.Name.IndexOf(_inputPath) + _inputPath.Length);
            string wavArtist = artistNameReg.Match(pathData).Value;
            string folderArtist = folderArtistNameReg.Match(pathData).Value;
            string fallbackArtist = string.IsNullOrWhiteSpace(wavArtist) ?
                string.IsNullOrWhiteSpace(folderArtist) ?
                    null :
                folderArtist :
            wavArtist;
            foreach (string artistName in _file.Tag.AlbumArtists)
            {
                if(!string.IsNullOrWhiteSpace(artistName))
                {
                    _data.artist.Add(new Artist
                    {
                        ArtistName = artistName
                    });
                }
            }
            //If album artist does not exist, duplicate person data into artist data, favoring performers since persons use composers then performers
            if (_data.artist.Count == 0)
            {
                foreach (string artistName in _file.Tag.Performers)
                {
                    if (!string.IsNullOrWhiteSpace(artistName))
                    {
                        _data.artist.Add(new Artist
                        {
                            ArtistName = artistName
                        });
                    }
                }
            } else
            {
                return; //we used the default album artists metadata
            }
            if(_data.artist.Count == 0)
            {
                foreach (string artistName in _file.Tag.Composers)
                {
                    if (!string.IsNullOrWhiteSpace(artistName))
                    {
                        _data.artist.Add(new Artist
                        {
                            ArtistName = artistName
                        });
                    }
                }
            } else
            {
                _logger.GenerationLogWriteData($"Artist: \"AlbumArtists\" metadata did not exist on track \"{_data.main.Title}\". Using \"Performers\" data instead...", true);
                return;
            }
            //If no relevant data exists in the file itself, use the path to figure it out
            if (_data.artist.Count == 0)
            {
                _data.artist.Add(new Artist
                {
                    ArtistName = fallbackArtist
                });
                _logger.GenerationLogWriteData($"Artist: \"AlbumArtists\" & \"Performers\" & \"Composers\" metadata did not exist on track \"{_data.main.Title}\". Using path/name-based data: \"{fallbackArtist}\"", true);
                return;
            } else
            {
                _logger.GenerationLogWriteData($"Artist: \"AlbumArtists\" & \"Performers\" metadata did not exist on track \"{_data.main.Title}\". Using \"Composers\" data instead...", true);
                return;
            }
        }
    }
}
