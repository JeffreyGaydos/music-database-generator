using System.IO;

namespace MusicDatabaseGenerator.Generators
{
    public class MainGenerator : AGenerator, IGenerator
    {
        public MainGenerator(TagLibFile file, MusicLibraryTrack track) {
            _file = file;
            _data = track;
        }

        public void Generate()
        {
            _data.main = new Main()
            {
                Title = _file.mp3.Tag.Title,
                FilePath = Path.GetFullPath(_file.path),
                Duration = (decimal)_file.mp3.Properties.Duration.TotalSeconds, //NOTE: Overflow exception possible, but unlikely
                ReleaseYear = (int?)_file.mp3.Tag.Year == 0 ? null : (int?)_file.mp3.Tag.Year, //NOTE: Overflow exception possible, but unlikely
                AddDate = new FileInfo(_file.path).LastWriteTime > new FileInfo(_file.path).CreationTime ? new FileInfo(_file.path).CreationTime : new FileInfo(_file.path).LastWriteTime,
            };
        }
    }
}
