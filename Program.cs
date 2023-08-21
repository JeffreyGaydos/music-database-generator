using MusicDatabaseGenerator.Generators;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MusicDatabaseGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LoggingUtils logger = new LoggingUtils();
            //TODO: Replace this later with a config or something
            //string musicFolder = "../../data3"; //point this to where the mp3s are located, relative to the top level folder of this repo
            //string musicFolder = "../../../../Music";
            string musicFolder = @"C:\Users\jeff1\Music";

            FolderReader.InjectDependencies(logger);
            (List<TagLib.File> tagFiles, List<(string, Bitmap)> coverArt) constructedTuple = FolderReader.ReadToTagLibFiles(musicFolder, true);

            MusicLibraryContext mdbContext = new MusicLibraryContext();
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int total = constructedTuple.tagFiles.Count;
            foreach (TagLib.File data in constructedTuple.tagFiles)
            {
                MusicLibraryTrack trackData = new MusicLibraryTrack(mdbContext, logger, total);

                List<IGenerator> generators = new List<IGenerator>
                {
                    new MainGenerator(data, trackData, logger),
                    new GenreGenerator(data, trackData),
                    new ArtistGenerator(data, trackData, musicFolder, logger),
                    new AlbumGenerator(data, trackData, musicFolder, logger),
                    new ArtistPersonGenerator(data, trackData, logger)
                };

                foreach (IGenerator generator in generators)
                {
                    generator.Generate();
                }

                MusicLibraryTrack.trackIndex += 1;

                trackData.Sync();
            }

            total = constructedTuple.coverArt.Count;
            logger.GenerationLogWriteComment($"Song Data Inserted Into Database in {sw.Elapsed.Seconds} seconds");
            sw.Restart();
            //Strict ordering, album art must come second so we can match it to an album via our sync function
            foreach ((string, Bitmap) img in constructedTuple.coverArt)
            {
                MusicLibraryTrack trackData = new MusicLibraryTrack(mdbContext, logger, total);

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
                    trackData.Sync();
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
            logger.GenerationLogWriteComment($"Album Art Data Inserted Into Database in {sw.Elapsed.Seconds} seconds");
        }
    }
}
