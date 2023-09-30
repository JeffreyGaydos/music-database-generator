using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class LinkedTrackSynchronizer : ASynchronizer
    {
        public LinkedTrackSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context, LoggingUtils logger)
        {
            _mlt = mlt;
            _context = context;
            _logger = logger;
        }

        public static new SyncOperation Delete()
        {
            if (_context.LinkedTracks.Where(mt => MainSynchonizer.TrackIDsDeleted.Contains(mt.TrackID1) || MainSynchonizer.TrackIDsDeleted.Contains(mt.TrackID2 ?? 0)).Any())
            {
                _logger.GenerationLogWriteData($"Deleted {_context.LinkedTracks.Where(mt => MainSynchonizer.TrackIDsDeleted.Contains(mt.TrackID1) || MainSynchonizer.TrackIDsDeleted.Contains(mt.TrackID2 ?? 0)).Count()} entries from LinkedTracks");
                _context.LinkedTracks.RemoveRange(_context.LinkedTracks.Where(mt => MainSynchonizer.TrackIDsDeleted.Contains(mt.TrackID1) || MainSynchonizer.TrackIDsDeleted.Contains(mt.TrackID2 ?? 0)));
                return SyncOperation.Delete;
            }
            return SyncOperation.None;
        }
    }
}
