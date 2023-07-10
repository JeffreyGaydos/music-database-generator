using System;

namespace MusicDatabaseGenerator.Generators
{
    public class ArtistGenerator : AGenerator, IGenerator
    {
        public ArtistGenerator(TagLib.File file, MusicLibraryTrack data)
        {
            _file = file;
            _data = data;
        }

        public void Generate()
        {
            foreach (string artistName in _file.Tag.AlbumArtists)
            {
                _data.artist.Add(new Artist
                {
                    ArtistName = artistName
                });
            }
        }
    }
}
