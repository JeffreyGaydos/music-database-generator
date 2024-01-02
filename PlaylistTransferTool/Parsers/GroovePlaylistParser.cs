﻿using MusicDatabaseGenerator;
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
                LoggingUtils.GenerationLogWriteData($"Successfully parsed Groove Music file {file}.");
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
                    var matchingTrack = ctx.Main.Where(t => t.FilePath.Contains(reducedSource)).FirstOrDefault();

                    //If that found nothing we need to use the title, duration, and artist to match on a track
                    if (matchingTrack == null)
                    {
                        var title = child.Attributes.Item(3).Value;
                        var duration = Math.Round(decimal.Parse(child.Attributes.Item(5).Value) / 1000);

                        matchingTrack = ctx.Main.Where(t => t.Title.Equals(title) && t.Duration == duration).FirstOrDefault();
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

            return plts.ToArray();
        }
    }
}
