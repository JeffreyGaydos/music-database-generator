using MusicDatabaseGenerator;
using MusicDatabaseGenerator.Synchronizers;
using PlaylistTransferTool.Synchronizers;
using System;
using Configurator = PlaylistTransferTool.MusicDatabaseGenerator.Configurator;
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

            foreach (var fileTuple in filesCategorized)
            {
                Playlist playlist = fileTuple.playlistParser.ParsePlaylist(fileTuple.fileName);
                var plSync = new PlaylistSynchronizer(playlist, mdbContext);
                var op = plSync.Insert();
                if(op == SyncOperation.Skip && config.mergePlaylistsWithSameName) //Skip op implies tht the playlist exists
                {
                    LoggingUtils.GenerationLogWriteData($"Merging playlist tracks for playlist {playlist.PlaylistName}");
                    PlaylistTracks[] playlistTracks = fileTuple.playlistParser.ParsePlaylistTracks(fileTuple.fileName, plSync.GetPlaylistID(), mdbContext);
                    var pltSync = new PlaylistTrackSynchronizer(playlistTracks, mdbContext);
                    pltSync.Insert();
                } else if(op == SyncOperation.Insert) //Insert op implies that the playlist is new
                {
                    PlaylistTracks[] playlistTracks = fileTuple.playlistParser.ParsePlaylistTracks(fileTuple.fileName, plSync.GetPlaylistID(), mdbContext);
                    var pltSync = new PlaylistTrackSynchronizer(playlistTracks, mdbContext);
                    pltSync.Insert();
                } else
                {
                    LoggingUtils.GenerationLogWriteData($"Skipping track updates for playlist {playlist.PlaylistName}");
                }

                if (config.playlistExportType != PlaylistType.None)
                {
                    LoggingUtils.GenerationLogWriteData($"Exporting to {Enum.GetName(typeof(PlaylistType), config.playlistExportType)} playlists.");
                    FolderReader._playlistParserMap.TryGetValue(config.playlistExportType, out var exportParser);
                    exportParser.Export(config.playlistExportPath, mdbContext);
                }
            }
        }
    }
}
