using MusicDatabaseGenerator.Generators;
using System;
using System.Collections.Generic;
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

            List<TagLib.File> tagFiles = FolderReader.ReadToTagLibFiles(musicFolder);

            MusicLibraryContext mdbContext = new MusicLibraryContext();

            foreach(TagLib.File data in tagFiles)
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
        }
    }
}
