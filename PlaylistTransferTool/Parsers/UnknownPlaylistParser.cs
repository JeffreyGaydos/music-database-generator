using MusicDatabaseGenerator;
using System;

namespace PlaylistTransferTool
{
    public class UnknownPlaylistParser : IPlaylistParser
    {
        public Playlist ParsePlaylist(string file)
        {
            throw new NotImplementedException();
        }

        public PlaylistTracks[] ParsePlaylistTracks(string file, int playlistID, MusicLibraryContext ctx)
        {
            throw new NotImplementedException();
        }

        public void Export(string exportPath, MusicLibraryContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}
