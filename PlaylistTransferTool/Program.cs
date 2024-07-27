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
            LoggingUtils.GenerationLogWriteData($"This tool works best when the file paths to your music are the same across devices, but this tool attempts to match if not");
            var config = new Configurator().HandleConfiguration();
            var filesCategorized = FolderReader.GetFiles(config.PlaylistImportPath);

            MusicLibraryContext mdbContext = new MusicLibraryContext();

            if(config.DeleteExistingPlaylists)
            {
                mdbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE [PlaylistTracks]");
                mdbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE [Playlist]");
                mdbContext.SaveChanges();
            }

            foreach ((var playlistParser, var fileName) in filesCategorized)
            {
                Playlist playlist = playlistParser.ParsePlaylist(fileName);
                var plSync = new PlaylistSynchronizer(playlist, mdbContext, config);
                var op = plSync.Sync();

                if(op != SyncOperation.Skip)
                {
                    List<PlaylistTracks> playlistTracks = playlistParser.ParsePlaylistTracks(fileName, plSync.GetPlaylistID(), mdbContext);
                    var pltSync = new PlaylistTrackSynchronizer(playlistTracks, mdbContext);
                    pltSync.Sync();
                }

                if (config.PlaylistExportType != PlaylistType.None)
                {
                    LoggingUtils.GenerationLogWriteData($"Exporting '{fileName}' as a '{Enum.GetName(typeof(PlaylistType), config.PlaylistExportType)}' playlist.");
                    FolderReader._playlistParserMap.TryGetValue(config.PlaylistExportType, out var exportParser);
                    exportParser.Export(config.PlaylistExportPath, mdbContext, plSync.GetPlaylistID());
                }
            }

            // So the user can just get all the playlists currently in the database if desired
            if (!filesCategorized.Any() && config.PlaylistExportType != PlaylistType.None)
            {
                LoggingUtils.GenerationLogWriteData($"Exporting existing playlists in database as '{Enum.GetName(typeof(PlaylistType), config.PlaylistExportType)}' playlists.");
                FolderReader._playlistParserMap.TryGetValue(config.PlaylistExportType, out var exportParser);
                exportParser.Export(config.PlaylistExportPath, mdbContext);
            }
            LoggingUtils.GenerationLogWriteData("Playlist Transfer Tool completed successfully.");
            LoggingUtils.Close();

            Console.WriteLine("Press any key to close this terminal...");
            Console.ReadKey();
        }
    }
}
