using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MusicDatabaseGenerator.Generators
{
    public class MainGenerator : AGenerator, IGenerator
    {
        private Regex reg = new Regex("(?<=[\\\\\\/])[^\\\\\\/]*(?=\\.[a-zA-Z]+)", RegexOptions.Compiled);
        private Regex titleReg = new Regex("(?<=- [0-9][0-9] )[^\\\\\\/]*(?=\\.[a-zA-Z]+)", RegexOptions.Compiled);
        public MainGenerator(TagLib.File file, MusicLibraryTrack track, LoggingUtils logger) {
            _file = file;
            _data = track;
            _logger = logger;
        }

        public void Generate()
        {
            //try to get just the title out of the file string, and if it's in an unexpected format, default to the whole filename
            string fileNameTitle = titleReg.Match(_file.Name).Value;
            fileNameTitle = string.IsNullOrWhiteSpace(fileNameTitle) ? reg.Match(_file.Name).Value : fileNameTitle;
            string title = string.IsNullOrWhiteSpace(_file.Tag.Title) ? fileNameTitle : _file.Tag.Title;
            if(_file.Tag.Title != title)
            {
                _logger.GenerationLogWriteData($"Title metadata did not exist on file \"{_file.Name}\" and was replaced with path-based name: \"{title}\"", true);
            }
            _data.main = new Main()
            {
                Title = PVU.PrevalidateStringTruncate(title, 435, nameof(Main.Title)),
                FilePath = PVU.PrevalidateStringTruncate(Path.GetFullPath(_file.Name), 260, nameof(Main.FilePath)),
                Duration = PVU.PrevalidateDoubleToDecimalCast(_file.Properties.Duration.TotalSeconds, nameof(Main.Duration)),
                ReleaseYear = (int?)PVU.PrevalidateUnsignedIntToIntCast(_file.Tag.Year, nameof(Main.ReleaseYear)) == 0 ? null : (int?)PVU.PrevalidateUnsignedIntToIntCast(_file.Tag.Year, nameof(Main.ReleaseYear)),
                AddDate = new FileInfo(_file.Name).CreationTime,
                LastModifiedDate = new FileInfo(_file.Name).LastWriteTime,
                Lyrics = PVU.PrevalidateStringTruncate(_file.Tag.Lyrics, 4000, nameof(Main.Lyrics)),
                Comment = PVU.PrevalidateStringTruncate(_file.Tag.Comment, 4000, nameof(Main.Comment)),
                BeatsPerMin = PVU.PrevalidateUnsignedIntToIntCast(_file.Tag.BeatsPerMinute, nameof(Main.BeatsPerMin)),
                Copyright = PVU.PrevalidateStringTruncate(_file.Tag.Copyright, 1000, nameof(Main.Copyright)),
                Publisher = PVU.PrevalidateStringTruncate(_file.Tag.Publisher, 1000, nameof(Main.Publisher)),
                ISRC = PVU.PrevalidateStringTruncate((_file.Tag.ISRC ?? "").Replace("-", ""), 12, nameof(Main.ISRC)), //some ISRCs are in the format A1-B2C-3D4-E5F6
                Bitrate = _file.Properties.AudioBitrate,
                Channels = _file.Properties.AudioChannels,
                SampleRate = _file.Properties.AudioSampleRate,
                BitsPerSample = _file.Properties.BitsPerSample,
                Owner = PVU.PrevalidateStringTruncate(System.Security.Principal.WindowsIdentity.GetCurrent().Name, 1000, nameof(Main.Owner))
            };
        }
    }
}
