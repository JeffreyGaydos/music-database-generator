using System.Text.RegularExpressions;

namespace MusicDatabaseGenerator.Generators
{
    public class AlbumGenerator : AGenerator, IGenerator
    {
        private Regex trackOrderRegex = new Regex("(?<= - )[0-9]+(?=[^\\\\\\/]*\\.[a-zA-Z]+)", RegexOptions.Compiled);
        private Regex albumNameRegex = new Regex("(?<= - )[^\\\\\\/]*(?= - [^\\\\\\/]*\\.[a-zA-Z]+)", RegexOptions.Compiled);
        private Regex parentFolderRegex = new Regex("(?<=[\\\\\\/])[^\\\\\\/]+(?=[\\\\\\/][^\\\\\\/]+\\.)", RegexOptions.Compiled);
        private string _inputPath;

        public AlbumGenerator(TagLib.File file, MusicLibraryTrack track, string inputPath, LoggingUtils logger)
        {
            _file = file;
            _data = track;
            _inputPath = inputPath;
            _logger = logger;
        }
        public void Generate()
        {
            int trackOrder = (int)_file.Tag.Track;
            trackOrder =
                trackOrder == 0
                     ? int.TryParse(trackOrderRegex.Match(_file.Name).Value, out trackOrder)
                        ? trackOrder
                        : 0
                    : trackOrder;
            string pathData = _file.Name.IndexOf(_inputPath) == -1 ? _file.Name : _file.Name.Substring(_file.Name.IndexOf(_inputPath) + _inputPath.Length);
            string wavAlbumName = albumNameRegex.Match(pathData).Value; //only should work for .wavs, if this fails (empty string), infer from the folder name
            string folderAlbumName = parentFolderRegex.Match(pathData).Value; //if we can't find it the "wav" way, then just use the parent folder name...
            string albumName = 
                string.IsNullOrWhiteSpace(wavAlbumName) ? 
                    string.IsNullOrWhiteSpace(folderAlbumName) ?
                        null
                    : folderAlbumName
                : wavAlbumName;

            _data.album.Add((new Album()
            {
                AlbumName = PVU.PrevalidateStringTruncate(string.IsNullOrWhiteSpace(_file.Tag.Album) ? albumName : _file.Tag.Album, 446, nameof(Album.AlbumName))
            }, new AlbumTracks()
            {
                TrackOrder = trackOrder,
            }));

            if (string.IsNullOrWhiteSpace(_file.Tag.Album))
            {
                _logger.GenerationLogWriteData($"Album: \"Album\" metadata did not exist on track \"{_data.main.Title}\". Using path based data: \"{albumName}\"", true);
            }

            if((int)_file.Tag.Track == 0)
            {
                if(trackOrder == 0)
                {
                    _logger.GenerationLogWriteData($"Album: \"Track Order\" & path-based track order metadata did not exist on track \"{_data.main.Title}\". Default to 0", true);
                } else
                {
                    _logger.GenerationLogWriteData($"Album: \"Track Order\" metadata did not exist on track \"{_data.main.Title}\", using path-based value \"{trackOrder}\"", true);
                }
            }
        }
    }
}
