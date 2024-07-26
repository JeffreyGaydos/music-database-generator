using MusicDatabaseGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using System.Xml;

namespace PlaylistTransferTool
{
    public class GroovePlaylistParser : IPlaylistParser
    {
        Regex titleRegex = new Regex("(?<=\\\\)[^\\\\\\/]+(?=\\.zpl)");
        Regex sourceRegex = new Regex(@"(?<=src\="")[^""]+", RegexOptions.Compiled);
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

            try
            {
                var zplFile = new StreamReader(file);
                var zpl = zplFile.ReadToEnd();

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(zpl);

                var xmlHead = xmlDoc.LastChild.FirstChild;
                result.PlaylistName = xmlHead.ChildNodes[3].InnerText;
            } catch (Exception ex)
            {
                LoggingUtils.GenerationLogWriteData($"WARNING: Could not parse Groove Music playlist file {file} or title element was not found.");
                LoggingUtils.GenerationLogWriteData($" ^-- {ex.Message}");
                LoggingUtils.GenerationLogWriteData($"WARNING: Using filename as playlist title");
                var match = titleRegex.Match(file);
                if(match.Success)
                {
                    LoggingUtils.GenerationLogWriteData($"WARNING: Used name \"{match.Value}\" as playlist title");
                } else
                {
                    LoggingUtils.GenerationLogWriteData("ERROR: Could not extract playlist title from path");
                }
            }

            return result;
        }

        public List<PlaylistTracks> ParsePlaylistTracks(string file, int playlistID, MusicLibraryContext ctx)
        {
            List<PlaylistTracks> plts = new List<PlaylistTracks>();
            try
            {
                var zplFile = new StreamReader(file);
                var zpl = zplFile.ReadToEnd();

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(zpl);

                var xmlSeq = xmlDoc.LastChild.LastChild.FirstChild;

                var trackIndex = 0;
                foreach (XmlNode child in xmlSeq.ChildNodes)
                {
                    var rawSource = child.Attributes.Item(0).Value;
                    var mat = trackNameRegex.Match(rawSource);
                    if(mat.Success)
                    {
                        var matchingTrack = ctx.Main.Where(t => t.FilePath.EndsWith(mat.Value)).FirstOrDefault();
                        //If that found nothing we need to use the title, duration, and artist to match on a track
                        if (matchingTrack == null)
                        {
                            var title = child.Attributes.Item(3).Value;
                            var duration = Math.Round(decimal.Parse(child.Attributes.Item(5).Value));

                            matchingTrack = ctx.Main.Where(t => t.Title.Equals(title) && (t.Duration >= duration - 1 && t.Duration <= duration + 1)).FirstOrDefault();
                        }

                        if (matchingTrack == null)
                        {
                            LoggingUtils.GenerationLogWriteData($"WARNING: Could not find track corresponding to path '{rawSource}' in existing database. Bogus TrackID assigned");
                            plts.Add(
                                new PlaylistTracks()
                                {
                                    PlaylistID = playlistID,
                                    TrackID = -trackIndex,
                                    TrackOrder = trackIndex,
                                    LastKnownPath = rawSource
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
                                    TrackOrder = trackIndex,
                                    LastKnownPath = rawSource
                                }
                            );
                        }
                        trackIndex++;
                    }
                }
            } catch (Exception ex)
            {
                LoggingUtils.GenerationLogWriteData($"ERROR: Could not parse Groove Music playlist file {file} or track data was not found.");
                LoggingUtils.GenerationLogWriteData($" ^-- {ex.Message}");
            }

            LoggingUtils.GenerationLogWriteData($"Finished parsing Groove Music Playlist {file}");
            return plts;
        }

        private class GroovePlaylist
        {
            public string title;
            public List<GrooveTrack> tracks;

            public string GetString()
            {
                var totalDuration = tracks.Sum(t => t.duration);
                var itemCount = tracks.Count;
                const string generator = "Entertainment Platform -- 10.20112.1011.0";
                const string zplHeader = "<?zpl version=\"2.0\"?>";

                var zpl = $@"{zplHeader}
<smil>
  <head>
    <meta name=""totalDuration"" content=""{totalDuration * 1000}"" />
    <meta name=""itemCount"" content=""{itemCount}"" />
    <meta name=""generator"" content=""{generator}"" />
    <title>Chill</title>
  </head>
  <body>
    <seq>";
                foreach(var track in tracks)
                {
                    zpl += $@"
      {track.GetString()}";
                }

                zpl += $@"
    </seq>
  </body>
</smil>";
                return zpl;
            }
        }

        private class GrooveTrack
        {
            public string source;
            public string albumTitle;
            public string albumArtist;
            public string trackTitle;
            public decimal duration;

            public string GetString()
            {
                return $"<media src=\"{source}\" albumTitle=\"{albumTitle}\" albumArtist=\"{albumArtist}\" trackTitle=\"{trackTitle}\" trackArtist=\"{albumArtist}\" duration=\"{duration * 1000}\" />";
            }
        }

        public void Export(string exportPath, MusicLibraryContext ctx)
        {
            List<GroovePlaylist> playlists = new List<GroovePlaylist>();

            foreach(var playlist in ctx.Playlist)
            {
                var groovePlaylist = new GroovePlaylist
                {
                    title = playlist.PlaylistName,
                    tracks = new List<GrooveTrack>()
                };

                foreach (var pt in ctx.PlaylistTracks.Where(pt => pt.PlaylistID == playlist.PlaylistID))
                {
                    var track = ctx.Main.FirstOrDefault(t => t.TrackID == pt.TrackID);
                    if (track == null)
                    {
                        var trackDict = ctx.Main.ToDictionary(t => t.FilePath, t => t.TrackID);
                        foreach(var filePath in trackDict.Keys)
                        {
                            if(MatchesByTrackName(filePath, pt.LastKnownPath))
                            {
                                track = ctx.Main.FirstOrDefault(t => t.TrackID == trackDict[filePath]);
                                break;
                            }
                        }
                    }
                    if (track == null)
                    {
                        LoggingUtils.GenerationLogWriteData($"ERROR: Could not find proper path to use in '{nameof(PlaylistType.Groove)}' playlist for '{playlist.PlaylistName}', track #{pt.TrackOrder} with last known path of '{pt.LastKnownPath}'");
                    }
                    else
                    {
                        var artistID = ctx.ArtistTracks.FirstOrDefault(at => at.TrackID == track.TrackID)?.ArtistID ?? 0;
                        var albumID = ctx.AlbumTracks.FirstOrDefault(at => at.TrackID == track.TrackID)?.AlbumID ?? 0;
                        groovePlaylist.tracks.Add(new GrooveTrack
                        {
                            albumArtist = ctx.Artist.Where(a => artistID == a.ArtistID).FirstOrDefault()?.ArtistName,
                            albumTitle = ctx.Album.Where(a => albumID == a.AlbumID).FirstOrDefault()?.AlbumName,
                            duration = track.Duration ?? 0,
                            source = track.FilePath,
                            trackTitle = track.Title,
                        });
                    }
                }

                playlists.Add(groovePlaylist);
            }

            foreach(var groovePlaylist in playlists)
            {
                var fileName = groovePlaylist.title + ".zpl";
                var contents = groovePlaylist.GetString();

                StreamWriter writer = new StreamWriter(exportPath + "\\" + fileName);
                writer.Write(contents);
                writer.Close();
            }
        }

        private bool MatchesByTrackName(string filePath, string lastKnownPath)
        {
            var mat = trackNameRegex.Match(lastKnownPath ?? "");
            var result = false;
            if(mat.Success)
            {
                result = filePath.EndsWith(mat.Value);
            }
            return result;
        }
    }
}
