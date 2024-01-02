using MusicDatabaseGenerator;
using System;

namespace PlaylistTransferTool
{
    public class SamsungPlaylistParser : IPlaylistParser
    {
        private LoggingUtils _logger;

        public SamsungPlaylistParser(LoggingUtils logger) {
            _logger = logger;
        }

        public Playlist ParsePlaylist(string file)
        {
            Playlist result = new Playlist()
            {
                CreationDate = DateTime.Now,
                LastEditDate = DateTime.Now,
                PlaylistDescription = $"Imported using the file {file} via the PlaylistTransferTool",
                PlaylistName = "Unknown Title",
            };


        }

        public PlaylistTracks[] ParsePlaylistTracks(string file, int playlistID, MusicLibraryContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}
