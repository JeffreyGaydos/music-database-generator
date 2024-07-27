using MusicDatabaseGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PlaylistTransferTool
{
    public class SamsungPlaylistParser : IPlaylistParser
    {
        Regex titleRegex = new Regex("(?<=\\\\)[^\\\\\\/]+(?=\\.m3u)", RegexOptions.Compiled);
        Regex trackNameRegex = new Regex(@"((?<=[/\\])[^\\/]+$)|(^[^/\\]+$)", RegexOptions.Compiled);
        Regex relevantPathPartRGX = new Regex(@"(?<=\\Music\\).+|^[^\\/]+$", RegexOptions.Compiled);

        public Playlist ParsePlaylist(string file)
        {
            Playlist result = new Playlist()
            {
                CreationDate = File.GetCreationTime(file),
                LastEditDate = File.GetLastWriteTime(file),
                PlaylistDescription = $"Imported using the file {file} via the PlaylistTransferTool",
                PlaylistName = "Unknown Title",
            };

            var match = titleRegex.Match(file);
            if(match.Success)
            {
                result.PlaylistName = match.Value;
            }

            return result;
        }

        public List<PlaylistTracks> ParsePlaylistTracks(string file, int playlistID, MusicLibraryContext ctx)
        {
            List<PlaylistTracks> plts = new List<PlaylistTracks>();
            try
            {
                StreamReader reader = new StreamReader(file);
                var contents = reader.ReadToEnd();
                var playlistItems = contents.Split('\n');
                playlistItems = playlistItems.Select(pi => pi.Replace("\r", "")).ToArray();

                var itemIndex = 0;
                foreach(var item in playlistItems)
                {
                    if (item == "#EXTM3U" || string.IsNullOrEmpty(item)) continue;
                    var mat = trackNameRegex.Match(item);
                    if (mat.Success)
                    {
                        var matchingTrack = ctx.Main.Where(t => t.FilePath.EndsWith(mat.Value)).FirstOrDefault();

                        if (matchingTrack == null)
                        {
                            LoggingUtils.GenerationLogWriteData($"WARNING: Could not find track corresponding to path '{item}' in existing database. Bogus TrackID assigned");
                            plts.Add(
                                new PlaylistTracks()
                                {
                                    PlaylistID = playlistID,
                                    TrackID = null,
                                    TrackOrder = itemIndex,
                                    LastKnownPath = item
                                }
                            );
                        }
                        else
                        {
                            plts.Add(
                                new PlaylistTracks()
                                {
                                    PlaylistID = playlistID,
                                    TrackID = matchingTrack.TrackID,
                                    TrackOrder = itemIndex,
                                    LastKnownPath = item
                                }
                            );
                        }
                        itemIndex++;
                    } else
                    {
                        LoggingUtils.GenerationLogWriteData($"ERROR: '{item}' does not appear to be a file (is it a directory?)");
                    }
                }
            } catch (Exception e) {
                LoggingUtils.GenerationLogWriteData($"Failure parsing Samsung playlist file {file}");
                LoggingUtils.GenerationLogWriteData($" ^-- {e.Message}");
            }
            
            LoggingUtils.GenerationLogWriteData($"Finished parsing Samsung Playlist {file}");
            return plts;
        }

        public void Export(string exportPath, MusicLibraryContext ctx)
        {
            string contents = "#EXTM3U";

            foreach(var playlist in ctx.Playlist)
            {
                var title = playlist.PlaylistName;
                foreach(var pt in ctx.PlaylistTracks.Where(pt => pt.PlaylistID == playlist.PlaylistID).OrderBy(pt => pt.TrackOrder))
                {
                    var track = ctx.Main.Where(t => t.TrackID == pt.TrackID).FirstOrDefault();
                    var mat = relevantPathPartRGX.Match(pt.LastKnownPath);
                    var relevantPath = mat.Success ? mat.Value : "";
                    if (track != null)
                    {
                        relevantPath = track.FilePath;
                        mat = relevantPathPartRGX.Match(relevantPath);
                    }
                    if(mat.Success && !string.IsNullOrWhiteSpace(mat.Value))
                    {
                        contents += $"\n{relevantPath}";
                    } else
                    {
                        LoggingUtils.GenerationLogWriteData($"ERROR: Could not find proper path to use in '{nameof(PlaylistType.Samsung)}' playlist for '{playlist.PlaylistName}', track #{pt.TrackOrder} with last known path of '{pt.LastKnownPath}'");
                    }
                }

                StreamWriter writer = new StreamWriter(exportPath + "\\" + title + ".m3u");
                writer.Write(contents);
                writer.Close();
            }
        }
    }
}
