using Microsoft.Extensions.Configuration;
using MusicDatabaseGenerator.Generators;
using MusicDatabaseGenerator.Synchronizers;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace MusicDatabaseGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile(Directory.GetParent("./") + "../../../appsettings.json").Build();
            
            var settings = config.GetSection("Settings").GetChildren().ToDictionary(r => r.Key, r => r.Value);

            MusicLibraryContext mdbContext = new MusicLibraryContext();
            string connectionString = mdbContext.Database.Connection.ConnectionString;
            string pathToSearch = settings["MusicFolderPathAbsolute"];
            bool generateAlbumArtData = settings["GenerateAlbumArtData"] == "True";
            bool generateMusicMetadata = settings["GenerateMusicMetadata"] == "True";
            bool regen = settings["DeleteDataOnGeneration"] == "True";

            LoggingUtils logger = new LoggingUtils();

            logger.GenerationLogWriteData("_CONFIGURATION:__________________________________");

            logger.GenerationLogWriteData($"Connecting to database via connection string \"{connectionString}\"...");
            logger.GenerationLogWriteData($"{(regen ? "Deleting existing data and resetting IDs..." : "Existing database persisted...")}");

            if (regen)
            {
                SqlConnection sqlConnection = new SqlConnection(connectionString);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = File.ReadAllText("../../Schema/db_delete.sql").Replace("\\r\\n", @"
").Replace("\\t", "  ");
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }

            logger.GenerationLogWriteData($"Searching for data at location \"{pathToSearch}\"");
            logger.GenerationLogWriteData($@"{(generateAlbumArtData ? 
                generateMusicMetadata ?
                    "Will generate music metadata and album art metadata"
                    : "Will generate album art metadata ONLY"
                : generateMusicMetadata ?
                    "Will generate music metadata ONLY"
                    : "Config was set to generate no data. Will still check for files with limited meatadata.")}");

            logger.GenerationLogWriteData("_________________________________________________");

            FolderReader.InjectDependencies(logger);
            (List<TagLib.File> tagFiles, List<(string, Bitmap)> coverArt) constructedTuple = FolderReader.ReadToTagLibFiles(pathToSearch, true);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (generateMusicMetadata)
            {
                int total = constructedTuple.tagFiles.Count;
                foreach (TagLib.File data in constructedTuple.tagFiles)
                {
                    MusicLibraryTrack trackData = new MusicLibraryTrack();
                    SyncManager syncManager = new SyncManager(mdbContext, logger, total, trackData);

                    List<IGenerator> generators = new List<IGenerator>
                    {
                        new MainGenerator(data, trackData, logger),
                        new GenreGenerator(data, trackData),
                        new ArtistGenerator(data, trackData, pathToSearch, logger),
                        new AlbumGenerator(data, trackData, pathToSearch, logger),
                        new ArtistPersonGenerator(data, trackData, logger)
                    };

                    foreach (IGenerator generator in generators)
                    {
                        generator.Generate();
                    }

                    MusicLibraryTrack.trackIndex += 1;

                    syncManager.Sync();
                }
                logger.GenerationLogWriteComment($"Song Data Inserted Into Database in {sw.Elapsed.TotalSeconds} seconds");
                sw.Restart();
            }

            if(generateAlbumArtData)
            {
                int total = constructedTuple.coverArt.Count;
                //Strict ordering, album art must come second so we can match it to an album via our sync function
                foreach ((string, Bitmap) img in constructedTuple.coverArt)
                {
                    MusicLibraryTrack trackData = new MusicLibraryTrack();
                    SyncManager syncManager = new SyncManager(mdbContext, logger, total, trackData);

                    List<IGenerator> generators = new List<IGenerator>
                    {
                        new AlbumArtGenerator(img.Item2, img.Item1, trackData),
                    };

                    foreach (IGenerator generator in generators)
                    {
                        generator.Generate();
                    }

                    MusicLibraryTrack.albumArtIndex += 1;
                
                    try
                    {
                        syncManager.Sync();
                    }
                    catch (UpdateException ue)
                    {
                        if (ue.Message.Contains("Violation of PRIMARY KEY constraint"))
                        {
                            logger.GenerationLogWriteData($"Found duplicate content for file, skipping {trackData.main.FilePath}");
                            logger.DuplicateLogWriteData(trackData.main.FilePath);
                        }
                    }
                }
                logger.GenerationLogWriteComment($"Album Art Data Inserted Into Database in {sw.Elapsed.TotalSeconds} seconds");
            }
        }
    }
}
