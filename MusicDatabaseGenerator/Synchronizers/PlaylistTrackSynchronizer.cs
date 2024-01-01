using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class PlaylistTrackSynchronizer : ASynchronizer
    {
        public PlaylistTrackSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context, LoggingUtils logger)
        {
            _mlt = mlt;
            _context = context;
            _logger = logger;
        }

        public static new SyncOperation Delete()
        {
            if (_context.PlaylistTracks.Where(mt => MainSynchonizer.TrackIDsDeleted.Contains(mt.TrackID)).Any())
            {
                _logger.GenerationLogWriteData($"Deleted {_context.PlaylistTracks.Where(mt => MainSynchonizer.TrackIDsDeleted.Contains(mt.TrackID)).Count()} entries from PlaylistTracks");
                _context.PlaylistTracks.RemoveRange(_context.PlaylistTracks.Where(mt => MainSynchonizer.TrackIDsDeleted.Contains(mt.TrackID)));
                return SyncOperation.Delete;
            }
            return SyncOperation.None;
        }
    }
}
