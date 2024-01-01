using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Linq;

namespace PlaylistTransferTool
{
    namespace MusicDatabaseGenerator
    {
        public enum PlaylistType
        {
            Unknown = 0,
            Groove = 1,
            Samsung = 2
        };

        public class ConfiguratorValues
        {
            public string playlistImportPath { get; private set; }
            public string playlistExportPath { get; private set; }
            public PlaylistType playlistExportType { get; private set; }
            public bool mergePlaylistsWithSameName { get; private set; }

            public ConfiguratorValues(string playlistImportPath, string playlistExportPath, PlaylistType playlistExportType, bool mergePlaylistsWithSameName)
            {
                this.playlistImportPath = playlistImportPath;
                this.playlistExportPath = playlistExportPath;
                this.playlistExportType = playlistExportType;
                this.mergePlaylistsWithSameName = mergePlaylistsWithSameName;
            }
        }

        public class Configurator
        {
            public Configurator()
            {

            }

            private PlaylistType StringToPlaylistType(string name)
            {
                if (Enum.TryParse<PlaylistType>(name, out var type))
                {
                    return type;
                }
                else
                {
                    Console.WriteLine($"ERROR: Playlist type {name} is not supported. Supported playlist tpyes are:");
                    foreach (var e in Enum.GetNames(typeof(PlaylistType)))
                    {
                        Console.WriteLine($" - {e}");
                    }
                    return PlaylistType.Unknown;
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
                    settings["mergePlaylistWithSameName"] == "True"
                );

                Console.WriteLine($"--== Ensure that your file structure on both of your devices is the same ==--");

                Console.WriteLine("_CONFIGURATION:__________________________________________________________________");
                Console.WriteLine($"Import Path: {values.playlistImportPath}");
                Console.WriteLine($"Export Path: {values.playlistExportPath}");
                Console.WriteLine($"Exporting \"{values.playlistExportType}\" playlists");
                Console.WriteLine($"Will merge playlists with the same name");

                return values;
            }
        }
    }
}
