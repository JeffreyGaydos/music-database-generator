﻿using MusicDatabaseGenerator;
using MusicDatabaseGenerator.Synchronizers;
using System.Collections.Generic;
using System.Linq;

namespace PlaylistTransferTool.Synchronizers
{
    public class PlaylistTrackSynchronizer
    {
        private List<PlaylistTracks> _playlistTracks;
        private MusicLibraryContext _context;

        public PlaylistTrackSynchronizer(List<PlaylistTracks> playlistTracks, MusicLibraryContext context)
        {
            _playlistTracks = playlistTracks;
            _context = context;
        }

        public SyncOperation Sync()
        {
            SyncOperation op = SyncOperation.None;
            foreach(var plt in _playlistTracks)
            {
                if(_context.PlaylistTracks.Where(pt => pt.TrackOrder == plt.TrackOrder && pt.PlaylistID == plt.PlaylistID).Any())
                {
                    var currentPt = _context.PlaylistTracks.FirstOrDefault(pt => pt.TrackOrder == plt.TrackOrder && pt.PlaylistID == plt.PlaylistID);
                    if (currentPt.LastKnownPath != plt.LastKnownPath || currentPt.TrackID != plt.TrackID)
                    {
                        op |= SyncOperation.Update;
                        LoggingUtils.GenerationLogWriteData($"Track with ID {plt.TrackID} is already in this playlist but has changes, updating");
                        //since our PK uses the track ID, we need to delete and re-insert rather than an in-place update

                        _context.PlaylistTracks.Remove(currentPt);
                        _context.PlaylistTracks.Add(plt);
                    } else
                    {
                        op |= SyncOperation.Skip;
                        LoggingUtils.GenerationLogWriteData($"Track with ID {plt.TrackID} is already in this playlist and has no changes, skipping");
                    }
                } else
                {
                    op |= SyncOperation.Insert;
                    if(_context.PlaylistTracks.Where(pt => pt.TrackID == plt.TrackID && pt.PlaylistID == plt.PlaylistID).Any())
                    {
                        LoggingUtils.GenerationLogWriteData($"Track with ID {plt.TrackID} is duplicated in this playlist, refusing to insert to prevent duplicates");
                    } else
                    {
                        _context.PlaylistTracks.Add(plt);
                    }
                }
            }

            _context.SaveChanges();

            return op;
        }
    }
}
