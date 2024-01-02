using PlaylistTransferTool.MusicDatabaseGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlaylistTransferTool
{
    public static class FolderReader
    {
        private static Dictionary<string, PlaylistType> _playlistExtensions = new Dictionary<string, PlaylistType>
        {
            { ".zpl", PlaylistType.Groove },
            { ".m3u", PlaylistType.Samsung }
        };

        private static Dictionary<PlaylistType, int> _playlistTypeCounts = new Dictionary<PlaylistType, int>
        {
            {PlaylistType.Unknown, 0},
            {PlaylistType.Groove, 0},
            {PlaylistType.Samsung, 0},
        };

        private static LoggingUtils _logger;

        private static UnknownPlaylistParser _nonParser = new UnknownPlaylistParser();
        private static GroovePlaylistParser _grooveParser = new GroovePlaylistParser(_logger);
        private static SamsungPlaylistParser _samsungParser = new SamsungPlaylistParser(_logger);

        private static Dictionary<PlaylistType, IPlaylistParser> _playlistParserMap = new Dictionary<PlaylistType, IPlaylistParser>
        {
            {PlaylistType.Groove, _grooveParser },            
            {PlaylistType.Samsung, _samsungParser }
        };

        public static void InjectDependencies(LoggingUtils logger)
        {
            _logger = logger;
        }

        public static (IPlaylistParser playlistParser, string fileName)[] GetFiles(string inputPath)
        {
            var result = Directory.GetFiles(inputPath).Select(file =>
            {
                if (_playlistExtensions.TryGetValue(file.Substring(file.Length - 4), out var type))
                {
                    _playlistTypeCounts[type] += 1;
                    return (playlistParser: _playlistParserMap[type], fileName: file);
                }
                else
                {
                    _playlistTypeCounts[PlaylistType.Unknown] += 1;
                    return (playlistParser: _nonParser, fileName: file);
                }
            }).ToArray();
            foreach(var key in _playlistTypeCounts.Keys)
            {
                _logger.GenerationLogWriteData($" * Found {_playlistTypeCounts[key]} \"{key}\" playlist files");
            }
            return result;
        }
    }
}
