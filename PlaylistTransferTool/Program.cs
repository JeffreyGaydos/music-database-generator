using MusicDatabaseGenerator;
using MusicDatabaseGenerator.Synchronizers;
using PlaylistTransferTool.Synchronizers;
using System;
using System.Collections.Generic;
using System.Linq;
using PlaylistTrackSynchronizer = PlaylistTransferTool.Synchronizers.PlaylistTrackSynchronizer;

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

            if(config.deleteExistingPlaylists)
            {
                mdbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE [PlaylistTracks]");
                mdbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE [Playlist]");
                mdbContext.SaveChanges();
            }

            foreach (var fileTuple in filesCategorized)
            {
                Playlist playlist = fileTuple.playlistParser.ParsePlaylist(fileTuple.fileName);
                var plSync = new PlaylistSynchronizer(playlist, mdbContext, config);
                var op = plSync.Sync();

                if(op != SyncOperation.Skip)
                {
                    List<PlaylistTracks> playlistTracks = fileTuple.playlistParser.ParsePlaylistTracks(fileTuple.fileName, plSync.GetPlaylistID(), mdbContext);
                    var pltSync = new PlaylistTrackSynchronizer(playlistTracks, mdbContext);
                    pltSync.Sync();
                }

                if (config.playlistExportType != PlaylistType.None)
                {
                    LoggingUtils.GenerationLogWriteData($"Exporting '{fileTuple.fileName}' as a '{Enum.GetName(typeof(PlaylistType), config.playlistExportType)}' playlist.");
                    FolderReader._playlistParserMap.TryGetValue(config.playlistExportType, out var exportParser);
                    exportParser.Export(config.playlistExportPath, mdbContext, plSync.GetPlaylistID());
                }
            }

            // So the user can just get all the playlists currently in the database if desired
            if (!filesCategorized.Any() && config.playlistExportType != PlaylistType.None)
            {
                LoggingUtils.GenerationLogWriteData($"Exporting existing playlists in database as '{Enum.GetName(typeof(PlaylistType), config.playlistExportType)}' playlists.");
                FolderReader._playlistParserMap.TryGetValue(config.playlistExportType, out var exportParser);
                exportParser.Export(config.playlistExportPath, mdbContext);
            }
        }
    }
}
