using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //TODO: Replace this later with a config or something
            string musicFolder = "data"; //point this to where the mp3s are located, relative to the top level folder of this repo
            /*
             * - musicFolder
             *      ∟ Artist name or album name
             *          ∟ Album name
             *          |    ∟ mp3 files
             *          ∟ mp3 files
             */

            List<string> SupportedExtensions = new List<string>
            {
                ".mp3",
                ".wav"
            };

            //FileStream stream = File.Open(musicPath);
            List<string> musicGroupings = Directory.GetDirectories("../../../" + musicFolder).ToList();
            List<string> musicFiles = new List<string>();
            foreach (string musicGroup in musicGroupings)
            {
                var files = Directory.GetFiles(musicGroup).ToList();
                if (files.Any(file => SupportedExtensions.Contains(file.Substring(file.LastIndexOf(".")))))
                {
                    Console.WriteLine("Artist+Album Found: " + musicGroup);
                    musicFiles.AddRange(files.Where(file => SupportedExtensions.Contains(file.Substring(file.LastIndexOf(".")))));
                }
                foreach (string musicSubgroup in Directory.GetDirectories(musicGroup))
                {
                    var subFiles = Directory.GetFiles(musicSubgroup).ToList();
                    Console.WriteLine("Artist+Album Found (SUB): " + musicGroup + ", Album: " + musicSubgroup);
                    musicFiles.AddRange(subFiles.Where(file => SupportedExtensions.Contains(file.Substring(file.LastIndexOf(".")))));
                }
            }
            foreach (string file in musicFiles)
            {
                //FileStream stream = File.OpenRead(file);
                var taglibfile = TagLib.File.Create(file);
                Console.WriteLine(taglibfile.Tag.Title);
            }
        }
    }
}
