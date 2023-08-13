using System;
using System.Text.RegularExpressions;

namespace MusicDatabaseGenerator.Generators
{
    public class ArtistGenerator : AGenerator, IGenerator
    {
        private Regex artistNameReg = new Regex("(?<=[\\\\\\/])[^\\\\\\/]*(?= - [^\\\\\\/]* - [^\\\\\\/]*\\.[a-zA-Z]+)", RegexOptions.Compiled);
        public ArtistGenerator(TagLib.File file, MusicLibraryTrack data)
        {
            _file = file;
            _data = data;
        }

        public void Generate()
        {
            foreach (string artistName in _file.Tag.AlbumArtists)
            {
                string artist = artistNameReg.Match(_file.Name).Value;
                artist = artist == "" ? null : artistNameReg.Match(_file.Name).Value;
                _data.artist.Add(new Artist
                {
                    ArtistName = artistName ?? artist
                });
            }
        }
    }
}
