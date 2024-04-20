using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Linq;
using MusicDatabaseGenerator;

namespace PlaylistTransferTool
{
    namespace MusicDatabaseGenerator
    {
        public class ConfiguratorValues
        {
            public string playlistImportPath { get; private set; }
            public string playlistExportPath { get; private set; }
            public PlaylistType playlistExportType { get; private set; }
            public bool mergePlaylistsWithSameName { get; private set; }
            public bool deleteExistingPlaylists { get; private set; }
            public DatabaseProvider databaseProvider { get; private set; }

            public ConfiguratorValues(string playlistImportPath, string playlistExportPath, PlaylistType playlistExportType, bool mergePlaylistsWithSameName, bool deleteExistingPlaylists, DatabaseProvider dbProdivder)
            {
                this.playlistImportPath = playlistImportPath;
                this.playlistExportPath = playlistExportPath;
                this.playlistExportType = playlistExportType;
                this.mergePlaylistsWithSameName = mergePlaylistsWithSameName;
                this.deleteExistingPlaylists = deleteExistingPlaylists;
                databaseProvider = dbProdivder;
            }
        }

        public class Configurator
        {
            private PlaylistType StringToPlaylistType(string name)
            {
                if (Enum.TryParse<PlaylistType>(name, out var type))
                {
                    return type;
                }
                else
                {
                    LoggingUtils.GenerationLogWriteData($"ERROR: Playlist type {name} is not supported. Supported playlist tpyes are:");
                    foreach (var e in Enum.GetNames(typeof(PlaylistType)))
                    {
                        LoggingUtils.GenerationLogWriteData($" ^-- {e}");
                    }
                    return PlaylistType.None;
                }
            }

            public ConfiguratorValues HandleConfiguration()
            {
                IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile(Directory.GetParent("./") + "../../../appsettings.json").Build();

                var settings = config.GetSection("Settings").GetChildren().ToDictionary(r => r.Key, r => r.Value);

                ConfiguratorValues values = new ConfiguratorValues(
                    settings["playlistImportPath"] ?? "",
                    settings["playlistExportPath"] ?? "",
                    StringToPlaylistType(settings["playlistExportType"] ?? ""),
                    settings["mergePlaylistWithSameName"] == "True",
                    settings["deleteExistingPlaylists"] == "True"
                );

                LoggingUtils.GenerationLogWriteData($"--== Ensure that your file structure on both of your devices is the same ==--");

                LoggingUtils.GenerationLogWriteData("_CONFIGURATION:__________________________________________________________________");
                LoggingUtils.GenerationLogWriteData($"Import Path: {values.playlistImportPath}");
                LoggingUtils.GenerationLogWriteData($"Export Path: {values.playlistExportPath}");
                LoggingUtils.GenerationLogWriteData($"Exporting \"{values.playlistExportType}\" playlists");
                LoggingUtils.GenerationLogWriteData($"Will merge playlists with the same name");
                LoggingUtils.GenerationLogWriteData($"Deleting existing playlists from database");
                LoggingUtils.GenerationLogWriteData("_________________________________________________________________________________");

                return values;
            }
        }
    }
}
