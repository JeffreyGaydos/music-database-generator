using MusicDatabaseGenerator;
using System;
using System.IO;
using System.Xml;

namespace PlaylistTransferTool
{
    public class GroovePlaylistParser : IPlaylistParser
    {
        public Playlist ParsePlaylist(string file)
        {
            Playlist result = new Playlist()
            {
                CreationDate = DateTime.Now,
                LastEditDate = DateTime.Now,
                PlaylistDescription = $"Imported using the file {file} via the PlaylistTransferTool",
                PlaylistName = "Unknown Title",
            };

            var zplFile = new StreamReader(file);
            var zpl = zplFile.ReadToEnd();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(zpl);

            var xmlHead = xmlDoc.LastChild.FirstChild;
            result.PlaylistName = xmlHead.ChildNodes[3].InnerText;

            return result;
        }

        public PlaylistTracks[] ParsePlaylistTracks(string file, int playlistID)
        {
            throw new NotImplementedException();
        }
    }
}
