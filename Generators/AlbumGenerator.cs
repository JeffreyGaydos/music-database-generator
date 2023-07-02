using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator.Generators
{
    public class AlbumGenerator : AGenerator, IGenerator
    {
        public AlbumGenerator(TagLibFile file, MusicLibraryTrack track)
        {
            _file = file;
            _data = track;
        }
        public void Generate()
        {
            _data.album.Add((new Album()
            {
                AlbumName = _file.mp3.Tag.Album
            }, new AlbumTracks()
            {
                TrackOrder = (int)_file.mp3.Tag.Track
            }));
        }
    }
}
