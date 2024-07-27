using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PlaylistTransferTool
{
    public static class FolderReader
    {
        private static readonly Regex _extensionRegex = new Regex(@"\.[^\.]+$", RegexOptions.Compiled);

        private static Dictionary<string, PlaylistType> _playlistExtensions = new Dictionary<string, PlaylistType>
        {
            { ".zpl", PlaylistType.Groove },
            { ".m3u", PlaylistType.Samsung },
            { ".m3u8", PlaylistType.Samsung }
        };

        private static Dictionary<PlaylistType, int> _playlistTypeCounts = new Dictionary<PlaylistType, int>
        {
            {PlaylistType.None, 0},
            {PlaylistType.Groove, 0},
            {PlaylistType.Samsung, 0},
        };

        private static UnknownPlaylistParser _nonParser = new UnknownPlaylistParser();
        private static GroovePlaylistParser _grooveParser = new GroovePlaylistParser();
        private static SamsungPlaylistParser _samsungParser = new SamsungPlaylistParser();

        public static Dictionary<PlaylistType, IPlaylistParser> _playlistParserMap = new Dictionary<PlaylistType, IPlaylistParser>
        {
            {PlaylistType.Groove, _grooveParser },            
            {PlaylistType.Samsung, _samsungParser }
        };

        public static (IPlaylistParser playlistParser, string fileName)[] GetFiles(string inputPath)
        {
            var result = Directory.GetFiles(inputPath).Select(file =>
            {
                if (_playlistExtensions.TryGetValue(_extensionRegex.Match(file).Value, out var type))
                {
                    _playlistTypeCounts[type] += 1;
                    return (playlistParser: _playlistParserMap[type], fileName: file);
                }
                else
                {
                    _playlistTypeCounts[PlaylistType.None] += 1;
                    return (playlistParser: _nonParser, fileName: file);
                }
            }).ToArray();
            foreach(var key in _playlistTypeCounts.Keys)
            {
                LoggingUtils.GenerationLogWriteData($" * Found {_playlistTypeCounts[key]} \"{key}\" playlist files");
            }
            LoggingUtils.GenerationLogWriteData("_________________________________________________________________________________");
            return result;
        }
    }
}
