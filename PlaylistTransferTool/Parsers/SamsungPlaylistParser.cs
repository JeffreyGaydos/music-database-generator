﻿using MusicDatabaseGenerator;
using System;

namespace PlaylistTransferTool
{
    public class SamsungPlaylistParser : IPlaylistParser
    {
        public Playlist ParsePlaylist(string file)
        {
            throw new NotImplementedException();
        }

        public PlaylistTracks[] ParsePlaylistTracks(string file, int playlistID, MusicLibraryContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}
