﻿using MusicDatabaseGenerator;
using System;
using System.Collections.Generic;

namespace PlaylistTransferTool
{
    public class UnknownPlaylistParser : IPlaylistParser
    {
        public Playlist ParsePlaylist(string file)
        {
            throw new NotImplementedException();
        }

        public List<(string trackPath, PlaylistTracks track)> ParsePlaylistTracks(string file, int playlistID, MusicLibraryContext ctx)
        {
            throw new NotImplementedException();
        }

        public void Export(string exportPath, MusicLibraryContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}
