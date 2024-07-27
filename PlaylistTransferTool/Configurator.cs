using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Linq;

namespace PlaylistTransferTool
{
    public class ConfiguratorValues
    {
        public string PlaylistImportPath { get; private set; }
        public string PlaylistExportPath { get; private set; }
        public PlaylistType PlaylistExportType { get; private set; }
        public bool MergePlaylistsWithSameName { get; private set; }
        public bool DeleteExistingPlaylists { get; private set; }

        public ConfiguratorValues(
            string playlistImportPath,
            string playlistExportPath,
            PlaylistType playlistExportType,
            bool mergePlaylistsWithSameName,
            bool deleteExistingPlaylists
        )
        {
            PlaylistImportPath = playlistImportPath;
            PlaylistExportPath = playlistExportPath;
            PlaylistExportType = playlistExportType;
            MergePlaylistsWithSameName = mergePlaylistsWithSameName;
            DeleteExistingPlaylists = deleteExistingPlaylists;
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

            LoggingUtils.GenerationLogWriteData("_CONFIGURATION:__________________________________________________________________");
            LoggingUtils.GenerationLogWriteData($"Will Import playlist files from: {values.PlaylistImportPath}");
            LoggingUtils.GenerationLogWriteData($"Will Export playlist files to: {values.PlaylistExportPath}");
            LoggingUtils.GenerationLogWriteData($"Will Export \"{values.PlaylistExportType}\" playlists");
            LoggingUtils.GenerationLogWriteData($"Will {(values.MergePlaylistsWithSameName ? "merge" : "overwrite")} playlists with the same name NOTE: config not implemented");
            LoggingUtils.GenerationLogWriteData($"Will {(values.DeleteExistingPlaylists ? "delete" : "persist")} existing playlists from database");
            LoggingUtils.GenerationLogWriteData("_________________________________________________________________________________");

            return values;
        }
    }
}
