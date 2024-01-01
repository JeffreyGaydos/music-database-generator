using MusicDatabaseGenerator;
using Configurator = PlaylistTransferTool.MusicDatabaseGenerator.Configurator;

namespace PlaylistTransferTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = new Configurator().HandleConfiguration();

            var filesCategorized = FolderReader.GetFiles(config.playlistImportPath);

            foreach (var fileTuple in filesCategorized)
            {
                Playlist playlist = fileTuple.playlistParser.ParsePlaylist(fileTuple.fileName);
                PlaylistTracks[] playlistTracks = fileTuple.playlistParser.ParsePlaylistTracks(fileTuple.fileName);
            }
        }
    }
}
