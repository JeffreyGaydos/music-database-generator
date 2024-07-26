using MusicDatabaseGenerator;
using MusicDatabaseGenerator.Synchronizers;
using System.Collections.Generic;
using System.Linq;

namespace PlaylistTransferTool.Synchronizers
{
    public class PlaylistTrackSynchronizer
    {
        private List<(string trackPath, PlaylistTracks track)> _playlistTracks;
        private MusicLibraryContext _context;

        public PlaylistTrackSynchronizer(List<(string trackPath, PlaylistTracks track)> playlistTracks, MusicLibraryContext context)
        {
            _playlistTracks = playlistTracks;
            _context = context;
        }

        public SyncOperation Insert()
        {
            SyncOperation op = SyncOperation.None;
            foreach(var plt in _playlistTracks)
            {
                if(_context.PlaylistTracks.Where(pt => pt.TrackID == plt.track.TrackID && pt.PlaylistID == plt.track.PlaylistID).Any())
                {
                    op |= SyncOperation.Skip;
                    LoggingUtils.GenerationLogWriteData($"Track with ID {plt.track.TrackID} is already in this playlist, skipping");
                } else
                {
                    op |= SyncOperation.Insert;
                    _context.PlaylistTracks.Add(plt.track);
                }
            }

            _context.SaveChanges();

            return op;
        }
    }
}
