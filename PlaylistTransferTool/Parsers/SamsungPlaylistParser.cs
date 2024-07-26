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

        public Playlist ParsePlaylist(string file)
        {
            Playlist result = new Playlist()
            {
                CreationDate = DateTime.Now,
                LastEditDate = DateTime.Now,
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

        public PlaylistTracks[] ParsePlaylistTracks(string file, int playlistID, MusicLibraryContext ctx)
        {
            List<PlaylistTracks> plts = new List<PlaylistTracks>();
            try
            {
                StreamReader reader = new StreamReader(file);
                var contents = reader.ReadToEnd();
                var playlistItems = contents.Split('\n');

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
                            plts.Add(new PlaylistTracks()
                            {
                                PlaylistID = playlistID,
                                TrackID = -itemIndex,
                                TrackOrder = itemIndex
                            });
                        }
                        else
                        {
                            plts.Add(new PlaylistTracks()
                            {
                                PlaylistID = playlistID,
                                TrackID = matchingTrack.TrackID,
                                TrackOrder = itemIndex
                            });
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
            return plts.ToArray();
        }

        Regex relevantPathPartRGX = new Regex("(?<=\\\\Music\\\\).+");

        public void Export(string exportPath, MusicLibraryContext ctx)
        {
            string contents = "#EXTM3U";

            foreach(var playlist in ctx.Playlist)
            {
                var title = playlist.PlaylistName;
                foreach(var pt in ctx.PlaylistTracks.Where(pt => pt.PlaylistID == playlist.PlaylistID))
                {
                    //TODO: Attempt to find the path if trackID is 0 (meaning we couldn't fnid the path in the database) ((wehn the config is on))
                    var relevantPath = relevantPathPartRGX.Match(ctx.Main.Where(t => t.TrackID == pt.TrackID).FirstOrDefault().FilePath);
                    contents += $@"
{relevantPath}";
                }

                StreamWriter writer = new StreamWriter(exportPath + "\\" + title + ".m3u");
                writer.Write(contents);
                writer.Close();
            }
        }
    }
}
