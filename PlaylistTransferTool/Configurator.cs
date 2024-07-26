﻿using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Linq;

namespace PlaylistTransferTool
{
    public class ConfiguratorValues
    {
        public string playlistImportPath { get; private set; }
        public string playlistExportPath { get; private set; }
        public PlaylistType playlistExportType { get; private set; }
        public bool mergePlaylistsWithSameName { get; private set; }
        public bool deleteExistingPlaylists { get; private set; }
        public bool findMissingTracks { get; private set; }

        public ConfiguratorValues(
            string playlistImportPath,
            string playlistExportPath,
            PlaylistType playlistExportType,
            bool mergePlaylistsWithSameName,
            bool deleteExistingPlaylists,
            bool findMissingTracks
        )
        {
            this.playlistImportPath = playlistImportPath;
            this.playlistExportPath = playlistExportPath;
            this.playlistExportType = playlistExportType;
            this.mergePlaylistsWithSameName = mergePlaylistsWithSameName;
            this.deleteExistingPlaylists = deleteExistingPlaylists;
            this.findMissingTracks = findMissingTracks;
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
                settings["deleteExistingPlaylists"] == "True",
                settings["exportOnly"] == "True",
                settings["findMissingTracks"] == "True"
            );

            LoggingUtils.GenerationLogWriteData($"--== Ensure that your file structure on both of your devices is the same ==--");

            LoggingUtils.GenerationLogWriteData("_CONFIGURATION:__________________________________________________________________");
            LoggingUtils.GenerationLogWriteData($"Will Import playlist files from: {values.playlistImportPath}");
            LoggingUtils.GenerationLogWriteData($"Will Export playlist files to: {values.playlistExportPath}");
            LoggingUtils.GenerationLogWriteData($"Will Export \"{values.playlistExportType}\" playlists");
            LoggingUtils.GenerationLogWriteData($"Will {(values.mergePlaylistsWithSameName ? "" : "NOT ")}merge playlists with the same name NOTE: config not implemented");
            LoggingUtils.GenerationLogWriteData($"Will {(values.deleteExistingPlaylists ? "delete" : "persist")} existing playlists from database");
            if(!values.findMissingTracks)
                LoggingUtils.GenerationLogWriteData($"Will attempt to find new paths for missing tracks. NOTE: NOT IMPLEMENTED");
            LoggingUtils.GenerationLogWriteData("_________________________________________________________________________________");

            return values;
        }
    }
}
