using MusicDatabaseGenerator.Generators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace MusicDatabaseGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //TODO: Replace this later with a config or something
            string musicFolder = "data2"; //point this to where the mp3s are located, relative to the top level folder of this repo
            //string musicFolder = "../../../../Music";

            (List<TagLib.File> tagFiles, List<(string, Bitmap)> coverArt) constructedTuple = FolderReader.ReadToTagLibFiles(musicFolder);

            MusicLibraryContext mdbContext = new MusicLibraryContext();

            foreach (TagLib.File data in constructedTuple.tagFiles)
            {
                MusicLibraryTrack trackData = new MusicLibraryTrack(mdbContext);

                List<IGenerator> generators = new List<IGenerator>
                {
                    new MainGenerator(data, trackData),
                    new GenreGenerator(data, trackData),
                    new ArtistGenerator(data, trackData),
                    new AlbumGenerator(data, trackData),
                    new ArtistPersonGenerator(data, trackData)
                };

                foreach (IGenerator generator in generators)
                {
                    generator.Generate();
                }

                MusicLibraryTrack.trackIndex += 1;

                trackData.Sync();
            }

            //Strict ordering, album art must come second so we can match it to an album via our sync function
            foreach ((string, Bitmap) img in constructedTuple.coverArt)
            {
                MusicLibraryTrack trackData = new MusicLibraryTrack(mdbContext);

                List<IGenerator> generators = new List<IGenerator>
                {
                    new AlbumArtGenerator(img.Item2, img.Item1, trackData),
                };

                foreach (IGenerator generator in generators)
                {
                    generator.Generate();
                }

                MusicLibraryTrack.albumArtIndex += 1;

                trackData.Sync();
                //TODO: Use regex to compare paths to determine which album the art is associated with
            }
        }
    }
}
