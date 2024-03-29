﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator
{
    public static class FolderReader
    {
        private static readonly List<string> _supportedMusicExtensions = new List<string>
        {
            ".mp3",
            ".flac",
            ".wav",
            ".m4a",
            ".wma"
        };

        private static readonly List<string> _limitedDataMusicExtensions = new List<string>
        {
            ".wav"
        };

        private static readonly List<string> _supportedAlbumArtExtensions = new List<string>
        {
            ".jpg",
            ".png"
        };

        private static List<string> _supportedMusicFiles = new List<string>();
        private static List<string> _supportedAlbumArtFiles = new List<string>();
        private static List<string> _partiallySupportedMusicFiles = new List<string>();
        private static List<string> _unsupportedFiles = new List<string>();

        private static readonly List<string> _allSupportedExtensions = _supportedMusicExtensions.Union(_supportedAlbumArtExtensions).ToList();
        
        private static LoggingUtils _logger;

        public static void InjectDependencies(LoggingUtils log)
        {
            _logger = log;
        }

        /**
         * ReadToTagLibFiles
         *      Reads through the folder and files found in the path parameter's location,
         *      relative to the top-level folder of this repo or an absolute path (if
         *      configured). The following are the expected file structures:
         *    
         *      Structure 1:
         *       - path
         *          ∟ Artist name and album name (folder)
         *            ∟ mp3 files
         *            ∟ cover art files
         *      
         *      Structure 2: 
         *       - path
         *         ∟ Artist name (folder)
         *            ∟ Album name (folder)
         *              ∟ mp3 files
         *              ∟ cover art files
         *       
         *       Any combination of these 2 structures in the same path folder is supported.
         */
        public static (List<TagLib.File> tagFiles, List<(string, Bitmap)> coverArt) ReadToTagLibFiles(string path, bool absolute = false)
        {
            List<TagLib.File> tagFiles = new List<TagLib.File>();
            List<(string, Bitmap)> coverArt = new List<(string, Bitmap)>();

            //FileStream stream = File.Open(musicPath);
            List<string> topLevelFolders = Directory.GetDirectories(absolute ? path : "../../" + path).ToList();

            _logger.LoadingBar("Gathering files", topLevelFolders, (toplevelFolder) =>
            {
                CategorizeFiles(toplevelFolder);

                foreach (string albumFolder in Directory.GetDirectories(toplevelFolder))
                {
                    CategorizeFiles(albumFolder);
                }
                return ""; //ignore output
            });

            _logger.LoadingBar("Parsing music files", _supportedMusicFiles, (file) =>
            {
                tagFiles.Add(TagLib.File.Create(file));
                return "";
            });

            _logger.LoadingBar("Parsing album art files", _supportedAlbumArtFiles, (file) =>
            {
                coverArt.Add((file, new Bitmap(file)));
                return "";
            });

            List<TagLib.File> corruptFiles = tagFiles.Where(t => t.PossiblyCorrupt).ToList();
            if (corruptFiles.Any())
            {
                _logger.GenerationLogWriteData("Files are possibly corrupt: ");
                foreach (TagLib.File corruptFile in corruptFiles)
                {
                    _logger.GenerationLogWriteData($"{corruptFile.Tag.Title}: {string.Join(",", corruptFile.CorruptionReasons.ToList())}");
                }
            }

            _logger.GenerationLogWriteData($"Found {_supportedMusicFiles.Count} music files and {_supportedAlbumArtFiles.Count} image files to process...");
            foreach(var partiallySupportedFile in _partiallySupportedMusicFiles)
            {
                _logger.LimitedDataLogWriteData(partiallySupportedFile);
            }
            _logger.GenerationLogWriteData($"{_partiallySupportedMusicFiles.Count} files had minimal metadata. Consider obtaining .mp3 or .flac version of the files found in ./files_with_limited_data.txt");

            return (tagFiles, coverArt);
        }

        private static void CategorizeFiles(string folderToSearch)
        {
            var files = Directory.GetFiles(folderToSearch).ToList();

            List<string> supportedMusic = files
                .Where(file => _supportedMusicExtensions
                    .Contains(ExtensionOf(file))).ToList();
            List<string> supportedImages = files
                .Where(file => _supportedAlbumArtExtensions
                    .Contains(ExtensionOf(file))).ToList();
            List<string> partiallySupportedFiles = files
                .Where(file => _limitedDataMusicExtensions
                    .Contains(ExtensionOf(file))).ToList();
            List<string> unsupportedFiles = files
                .Where(file => !_allSupportedExtensions
                    .Contains(ExtensionOf(file))).ToList();

            _supportedMusicFiles.AddRange(supportedMusic);
            _supportedAlbumArtFiles.AddRange(supportedImages);
            _partiallySupportedMusicFiles.AddRange(partiallySupportedFiles);
            _unsupportedFiles.AddRange(unsupportedFiles);
        }

        private static string ExtensionOf(string filePath)
        {
            if (filePath.LastIndexOf(".") == -1) return "";
            return filePath.Substring(filePath.LastIndexOf("."));
        }
    }
}
