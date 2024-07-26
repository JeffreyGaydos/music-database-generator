using MusicDatabaseGenerator;
using System.Collections.Generic;

namespace PlaylistTransferTool
{
    public interface IPlaylistParser
    {
        Playlist ParsePlaylist(string file);

        List<(string trackPath, PlaylistTracks track)> ParsePlaylistTracks(string file, int playlistID, MusicLibraryContext ctx);

        void Export(string exportPath, MusicLibraryContext ctx);
    }
}
