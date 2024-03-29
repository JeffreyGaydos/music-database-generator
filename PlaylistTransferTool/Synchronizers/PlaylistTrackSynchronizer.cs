﻿using MusicDatabaseGenerator;
using MusicDatabaseGenerator.Synchronizers;
using System.Linq;

namespace PlaylistTransferTool.Synchronizers
{
    public class PlaylistTrackSynchronizer
    {
        private PlaylistTracks[] _playlistTracks;
        private MusicLibraryContext _context;

        public PlaylistTrackSynchronizer(PlaylistTracks[] playlistTracks, MusicLibraryContext context)
        {
            _playlistTracks = playlistTracks;
            _context = context;
        }

        public SyncOperation Insert()
        {
            SyncOperation op = SyncOperation.None;
            foreach(var plt in _playlistTracks)
            {
                if(_context.PlaylistTracks.Where(pt => pt.TrackID == plt.TrackID && pt.PlaylistID == plt.PlaylistID).Any())
                {
                    op |= SyncOperation.Skip;
                    LoggingUtils.GenerationLogWriteData($"Track with ID {plt.TrackID} is already in this playlist, skipping");
                } else
                {
                    op |= SyncOperation.Insert;
                    _context.PlaylistTracks.Add(plt);
                }
            }

            _context.SaveChanges();

            return op;
        }
    }
}
