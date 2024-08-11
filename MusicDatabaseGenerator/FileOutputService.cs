using MusicDatabaseGenerator.Synchronizers;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MusicDatabaseGenerator
{
    public class FileOutputService
    {
        private List<string> _tracksToOutput = new List<string>();
        private List<string> _imagesToOutput = new List<string>();

        private string _trackOutputPath;
        private string _imageOutputPath;
        private string _relativeToPath;
        private LoggingUtils _logger;

        private readonly Regex _afterRelativePathRegex;

        public FileOutputService(ConfiguratorValues config, LoggingUtils logger)
        {
            _trackOutputPath = config.trackOutputPath;
            _imageOutputPath = config.imageOutputPath;
            var sanitizedPathToSearch = config.pathToSearch.Replace("\\", "/");
            _relativeToPath = sanitizedPathToSearch.Substring(sanitizedPathToSearch.LastIndexOf("/") + 1);
            _relativeToPath = string.IsNullOrWhiteSpace(_relativeToPath) ? "Music" : _relativeToPath;
            _afterRelativePathRegex = new Regex($@"(?<={_relativeToPath.Replace("/", "")}[\\/]).+", RegexOptions.Compiled);
            _logger = logger;
        }

        public void LoadSynchronizedTrack(SyncOperation op, string trackPath)
        {
            if(!string.IsNullOrWhiteSpace(_trackOutputPath))
            {
                if ((op & SyncOperation.Insert) > 0 || (op & SyncOperation.Update) > 0)
                {
                    _tracksToOutput.Add(trackPath);
                }
            }
        }

        public void LoadSynchronizedImage(SyncOperation op, string imagePath)
        {
            if(!string.IsNullOrWhiteSpace(_imageOutputPath))
            {
                if ((op & SyncOperation.Insert) > 0 || (op & SyncOperation.Update) > 0)
                {
                    _tracksToOutput.Add(imagePath);
                }
            }
        }

        public void Export()
        {
            if(!string.IsNullOrWhiteSpace(_trackOutputPath))
            {
                _logger.GenerationLogWriteData($"Exporting tracks with directory structure to {_trackOutputPath}");
                ExportTracks();
            }
            if(!string.IsNullOrWhiteSpace(_imageOutputPath))
            {
                _logger.GenerationLogWriteData($"Exporting images with directory structure to {_imageOutputPath}");
                ExportImages();
            }
        }

        private void ExportImages()
        {
            foreach (var imagePath in _imagesToOutput)
            {
                var relativeMatch = _afterRelativePathRegex.Match(imagePath);
                if (relativeMatch.Success)
                {
                    var writePath = Path.Combine(_imageOutputPath, relativeMatch.Value);
                    writePath = writePath.Replace("\\", "/");
                    string folder = writePath.Substring(0, writePath.LastIndexOf("/"));
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    if(File.Exists(writePath))
                    {
                        File.Delete(writePath); //overwrite existing files, but merge updates otherwise
                    }
                    File.Copy(imagePath, writePath);
                }
                else
                {
                    _logger.GenerationLogWriteData($"WARNING: Image path \"{imagePath}\" did not seem to be contained in the directory \"{_relativeToPath}\"");
                }
            }
        }

        private void ExportTracks()
        {
            foreach(var trackPath in _tracksToOutput)
            {
                var relativeMatch = _afterRelativePathRegex.Match(trackPath);
                if(relativeMatch.Success)
                {
                    var writePath = Path.Combine(_trackOutputPath, relativeMatch.Value);
                    writePath = writePath.Replace("\\", "/");
                    string folder = writePath.Substring(0, writePath.LastIndexOf("/"));
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    if (File.Exists(writePath))
                    {
                        File.Delete(writePath); //overwrite existing files, but merge updates otherwise
                    }
                    File.Copy(trackPath, writePath);
                } else
                {
                    _logger.GenerationLogWriteData($"WARNING: Track path \"{trackPath}\" did not seem to be contained in the directory \"{_relativeToPath}\"");
                }
            }
        }
    }
}
