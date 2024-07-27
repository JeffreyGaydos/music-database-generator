using MusicDatabaseGenerator;
using System.Collections.Generic;

namespace PlaylistTransferTool
{
    public interface IPlaylistParser
    {
        Playlist ParsePlaylist(string file);

        List<PlaylistTracks> ParsePlaylistTracks(string file, int playlistID, MusicLibraryContext ctx);

        void Export(string exportPath, MusicLibraryContext ctx, int? playlistIdFilter = null);
    }
}
