using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator.Generators
{
    public class AlbumGenerator : AGenerator, IGenerator
    {
        public AlbumGenerator(TagLib.File file, MusicLibraryTrack track)
        {
            _file = file;
            _data = track;
        }
        public void Generate()
        {
            _data.album.Add((new Album()
            {
                AlbumName = _file.Tag.Album
            }, new AlbumTracks()
            {
                TrackOrder = (int)_file.Tag.Track
            }));
        }
    }
}
