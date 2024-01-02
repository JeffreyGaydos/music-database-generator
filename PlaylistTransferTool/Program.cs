using MusicDatabaseGenerator;
using PlaylistTransferTool.Synchronizers;
using Configurator = PlaylistTransferTool.MusicDatabaseGenerator.Configurator;

namespace PlaylistTransferTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LoggingUtils.Init();
            var config = new Configurator().HandleConfiguration();
            var filesCategorized = FolderReader.GetFiles(config.playlistImportPath);

            MusicLibraryContext mdbContext = new MusicLibraryContext();

            foreach (var fileTuple in filesCategorized)
            {
                Playlist playlist = fileTuple.playlistParser.ParsePlaylist(fileTuple.fileName);
                var plSync = new PlaylistSynchronizer(playlist, mdbContext);
                plSync.Insert();
                PlaylistTracks[] playlistTracks = fileTuple.playlistParser.ParsePlaylistTracks(fileTuple.fileName, plSync.GetPlaylistID(), mdbContext);
                var pltSync = new PlaylistTrackSynchronizer(playlistTracks, mdbContext);
                pltSync.Insert();
            }
        }
    }
}
