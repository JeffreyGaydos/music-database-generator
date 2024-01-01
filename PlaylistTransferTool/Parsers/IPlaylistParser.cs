using MusicDatabaseGenerator;

namespace PlaylistTransferTool
{
    public interface IPlaylistParser
    {
        Playlist ParsePlaylist(string file);

        PlaylistTracks[] ParsePlaylistTracks(string file, int playlistID);
    }
}
