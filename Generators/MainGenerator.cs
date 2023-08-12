using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MusicDatabaseGenerator.Generators
{
    public class MainGenerator : AGenerator, IGenerator
    {
        private Regex reg = new Regex("(?<=[\\\\\\/])[^\\\\\\/]*(?=\\.[a-zA-Z]+)", RegexOptions.Compiled);

        public MainGenerator(TagLib.File file, MusicLibraryTrack track) {
            _file = file;
            _data = track;
        }

        public void Generate()
        {
            string fileNameTitle = reg.Matches(_file.Name)[0].Value;
            string title = _file.Tag.Title ?? fileNameTitle;
            if(_file.Tag.Title != title)
            {
                Console.WriteLine($"Title metadata did not exist on file \"{_file.Name}\" and was replaced with path-based name: \"{title}\"");
            }
            _data.main = new Main()
            {
                Title = title,
                FilePath = Path.GetFullPath(_file.Name),
                Duration = (decimal)_file.Properties.Duration.TotalSeconds, //NOTE: Overflow exception possible, but unlikely
                ReleaseYear = (int?)_file.Tag.Year == 0 ? null : (int?)_file.Tag.Year, //NOTE: Overflow exception possible, but unlikely
                AddDate = new FileInfo(_file.Name).LastWriteTime > new FileInfo(_file.Name).CreationTime ? new FileInfo(_file.Name).CreationTime : new FileInfo(_file.Name).LastWriteTime,
                Lyrics = _file.Tag.Lyrics,
                Comment = _file.Tag.Comment,
                BeatsPerMin = (int)_file.Tag.BeatsPerMinute,
                Copyright = _file.Tag.Copyright,
                Publisher = _file.Tag.Publisher,
                ISRC = _file.Tag.ISRC,
                Bitrate = _file.Properties.AudioBitrate,
                Channels = _file.Properties.AudioChannels,
                SampleRate = _file.Properties.AudioSampleRate,
                BitsPerSample = _file.Properties.BitsPerSample,
                Owner = System.Security.Principal.WindowsIdentity.GetCurrent().Name
            };
        }
    }
}
