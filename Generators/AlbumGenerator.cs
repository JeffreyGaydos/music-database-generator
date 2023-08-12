using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator.Generators
{
    public class AlbumGenerator : AGenerator, IGenerator
    {
        private Regex trackOrderRegex = new Regex("(?<= - )[0-9]+(?=[^\\\\\\/]*\\.[a-zA-Z]+)", RegexOptions.Compiled);
        private Regex albumNameRegex = new Regex("(?<= - )[^\\\\\\/]*(?= - [^\\\\\\/]*\\.[a-zA-Z]+)", RegexOptions.Compiled);

        public AlbumGenerator(TagLib.File file, MusicLibraryTrack track)
        {
            _file = file;
            _data = track;
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

            string albumName = albumNameRegex.Match(_file.Name).Value;
            albumName = albumName == "" ? null : albumName;

            _data.album.Add((new Album()
            {
                AlbumName = _file.Tag.Album ?? albumName
            }, new AlbumTracks()
            {
                TrackOrder = trackOrder,
            }));
        }
    }
}
