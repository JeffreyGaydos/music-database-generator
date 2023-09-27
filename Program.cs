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
            MusicLibraryContext mdbContext = new MusicLibraryContext();
            LoggingUtils logger = new LoggingUtils();

            Configurator configHandle = new Configurator(mdbContext, logger);
            ConfiguratorValues config = configHandle.HandleConfiguration();

            FolderReader.InjectDependencies(logger);
            (List<TagLib.File> tagFiles, List<(string, Bitmap)> coverArt) constructedTuple = FolderReader.ReadToTagLibFiles(config.pathToSearch, true);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (config.generateMusicMetadata)
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
                        new ArtistGenerator(data, trackData, config.pathToSearch, logger),
                        new AlbumGenerator(data, trackData, config.pathToSearch, logger),
                        new ArtistPersonGenerator(data, trackData, logger)
                    };

                    foreach (IGenerator generator in generators)
                    {
                        generator.Generate();
                    }

                    MusicLibraryTrack.trackIndex += 1;

                    syncManager.Sync();
                }
                SyncManager.Delete();
                logger.GenerationLogWriteComment($"Song Data Inserted Into Database in {sw.Elapsed.TotalSeconds} seconds");
                sw.Restart();
            }

            if(config.generateAlbumArtData)
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
