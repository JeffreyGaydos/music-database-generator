using MusicDatabaseGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace PlaylistTransferTool
{
    public class GroovePlaylistParser : IPlaylistParser
    {
        Regex titleRegex = new Regex("(?<=\\\\)[^\\\\\\/]+(?=\\.zpl)");

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
                LoggingUtils.GenerationLogWriteData($"ERROR: Could not parse Groove Music playlist file {file} or title element was not found.");
                LoggingUtils.GenerationLogWriteData($" ^-- {ex.Message}");
                LoggingUtils.GenerationLogWriteData($"Using filename as playlist title");
                var match = titleRegex.Match(file);
                if(match.Success)
                {
                    LoggingUtils.GenerationLogWriteData($"Used name \"{match.Value}\" as playlist title");
                } else
                {
                    LoggingUtils.GenerationLogWriteData("ERROR: Could not extract playlist title from path");
                }
            }

            return result;
        }

        public PlaylistTracks[] ParsePlaylistTracks(string file, int playlistID, MusicLibraryContext ctx)
        {
            List<PlaylistTracks> plts = new List<PlaylistTracks>();
            try
            {
                var zplFile = new StreamReader(file);
                var zpl = zplFile.ReadToEnd();

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(zpl);

                var xmlSeq = xmlDoc.LastChild.LastChild.FirstChild;

                var childIndex = 0;
                foreach (XmlNode child in xmlSeq.ChildNodes)
                {
                    var rawSource = child.Attributes.Item(0).Value;
                    var reducedSource = rawSource.Substring(rawSource.IndexOf("\\Music\\") + 7);

                    //first we assume that the paths are the same
                    reducedSource = new UniversalPathComparator().ToWindowsPath(reducedSource);
                    var matchingTrack = ctx.Main.Where(t => t.FilePath.Contains(reducedSource)
                    ).FirstOrDefault();

                    //If that found nothing we need to use the title, duration, and artist to match on a track
                    if (matchingTrack == null)
                    {
                        var title = child.Attributes.Item(3).Value;
                        var duration = Math.Round(decimal.Parse(child.Attributes.Item(5).Value));

                        matchingTrack = ctx.Main.Where(t => t.Title.Equals(title) && (t.Duration >= duration - 1 && t.Duration <= duration + 1)).FirstOrDefault();
                    }

                    if (matchingTrack == null)
                    {
                        LoggingUtils.GenerationLogWriteData($"ERROR: Could not find track corresponding to path {rawSource} in existing database");
                    }
                    else
                    {
                        plts.Add(new PlaylistTracks()
                        {
                            PlaylistID = playlistID,
                            TrackID = matchingTrack.TrackID,
                            TrackOrder = childIndex
                        });
                    }
                    childIndex++;
                }
            } catch (Exception ex)
            {
                LoggingUtils.GenerationLogWriteData($"ERROR: Could not parse Groove Music playlist file {file} or track data was not found.");
                LoggingUtils.GenerationLogWriteData($" ^-- {ex.Message}");
            }

            LoggingUtils.GenerationLogWriteData($"Finished parsing Groove Music Playlist {file}");
            return plts.ToArray();
        }

        private class GroovePlaylist
        {
            public string title;
            public GrooveTrack[] tracks;

            public string GetString()
            {
                var totalDuration = tracks.Sum(t => t.duration);
                var itemCount = tracks.Length;
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

            foreach(var list in ctx.Playlist)
            {
                var playlist = new GroovePlaylist
                {
                    title = list.PlaylistName,
                    tracks = ctx.PlaylistTracks
                        .Where(pt => pt.PlaylistID == list.PlaylistID)
                        .Select(pt => new GrooveTrack
                        {
                            albumArtist = ctx.Artist.Where(a => ctx.ArtistTracks.Where(at => at.TrackID == pt.TrackID).FirstOrDefault().ArtistID == a.ArtistID).FirstOrDefault().ArtistName,
                            albumTitle = ctx.Album.Where(a => ctx.AlbumTracks.Where(at => at.TrackID == pt.TrackID).FirstOrDefault().AlbumID == a.AlbumID).FirstOrDefault().AlbumName,
                            duration = ctx.Main.Where(t => t.TrackID == pt.TrackID).FirstOrDefault().Duration ?? 0,
                            source = ctx.Main.Where(t => t.TrackID == pt.TrackID).FirstOrDefault().FilePath,
                            trackTitle = ctx.Main.Where(t => t.TrackID == pt.TrackID).FirstOrDefault().Title,
                        }).ToArray(),
                };

                playlists.Add(playlist);
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
    }
}
