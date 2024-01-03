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
        Regex titleRegex = new Regex("(?<=\\\\)[^\\\\\\/]+(?=\\.m3u)");

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
                    if (item == "#EXTM3U") continue;
                    var reducedSource = item;

                    //we assume that the paths are the same. There is no other data to go off of for this...
                    reducedSource = new UniversalPathComparator().ToWindowsPath(reducedSource);
                    var matchingTrack = ctx.Main.Where(t => t.FilePath.Contains(reducedSource)).FirstOrDefault();

                    if (matchingTrack == null)
                    {
                        LoggingUtils.GenerationLogWriteData($"ERROR: Could not find track corresponding to path {item} in existing database");
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
