using MusicDatabaseGenerator.Generators;
using MusicDatabaseGenerator.Synchronizers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace MusicDatabaseGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MusicLibraryContext mdbContext = new MusicLibraryContext();
            LoggingUtils logger = new LoggingUtils();
            PVU.Init(logger);

            Configurator configHandle = new Configurator(mdbContext, logger);
            ConfiguratorValues config = configHandle.HandleConfiguration();

            FolderReader.InjectDependencies(logger);
            (List<TagLib.File> tagFiles, List<(string, Bitmap)> coverArt) constructedTuple = FolderReader.ReadToTagLibFiles(config.pathToSearch, true);

            FileOutputService exportService = new FileOutputService(config, logger);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (config.generateMusicMetadata)
            {
                int total = constructedTuple.tagFiles.Count;
                foreach (TagLib.File data in constructedTuple.tagFiles)
                {
                    MusicLibraryTrack trackData = new MusicLibraryTrack();
                    SyncManager syncManager = new SyncManager(mdbContext, logger, total, trackData, false);

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
                    exportService.LoadSynchronizedTrack(syncManager.Sync(), trackData.main.FilePath);
                }
                new PostProcessingSynchronizer(mdbContext).Synchronize(); //external for performance reasons
                SyncManager.Delete();
                logger.GenerationLogWriteData($"Inserted {SyncManager.Inserts} record(s)");
                logger.GenerationLogWriteData($"Updated {SyncManager.Updates} record(s)");
                logger.GenerationLogWriteData($"Skipped {SyncManager.Skips} record(s)");
                logger.GenerationLogWriteComment($"Song Data Inserted Into Database in {sw.Elapsed.TotalSeconds} seconds");
                sw.Restart();
            }

            SyncManager.Inserts = 0;
            SyncManager.Updates = 0;
            SyncManager.Skips = 0;

            if (config.generateAlbumArtData)
            {
                int total = constructedTuple.coverArt.Count;
                //Strict ordering, album art must come second so we can match it to an album via our sync function
                foreach ((string, Bitmap) img in constructedTuple.coverArt)
                {
                    MusicLibraryTrack trackData = new MusicLibraryTrack();
                    SyncManager syncManager = new SyncManager(mdbContext, logger, total, trackData, true);

                    List<IGenerator> generators = new List<IGenerator>
                    {
                        new AlbumArtGenerator(img.Item2, img.Item1, trackData),
                    };

                    foreach (IGenerator generator in generators)
                    {
                        generator.Generate();
                    }

                    MusicLibraryTrack.albumArtIndex += 1;

                    syncManager.Sync();
                    exportService.LoadSynchronizedImage(img.Item1);
                }
                SyncManager.Delete();
                logger.GenerationLogWriteData($"Inserted {SyncManager.Inserts} record(s)");
                logger.GenerationLogWriteData($"Updated {SyncManager.Updates} record(s)");
                logger.GenerationLogWriteData($"Skipped {SyncManager.Skips} record(s)");
                logger.GenerationLogWriteComment($"Album Art Data Inserted Into Database in {sw.Elapsed.TotalSeconds} seconds");
            }

            exportService.Export();

            logger.GenerationLogWriteData("Music Database Generator completed successfully.");

            Console.WriteLine("Press any key to close this terminal...");
            Console.ReadKey();
        }
    }
}
