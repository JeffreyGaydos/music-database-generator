namespace MusicDatabaseGenerator.Generators
{
    public class GenreGenerator : AGenerator, IGenerator
    {
        public GenreGenerator(TagLibFile file, MusicLibraryTrack data) {
            _file = file;
            _data = data;
        }

        public void Generate()
        {
            foreach (string genreName in _file.mp3.Tag.Genres)
            {
                _data.genre.Add(new Genre
                {
                    GenreName = genreName
                });
            }
        }        
    }
}
