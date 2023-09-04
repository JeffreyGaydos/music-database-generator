using System.Collections.Generic;
using System.Linq;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class SyncManager
    {
        private MusicLibraryContext _context;
        private LoggingUtils _logger;
        private int _totalCount;
        private MusicLibraryTrack _mlt;

        public SyncManager(MusicLibraryContext context, LoggingUtils logger, int count, MusicLibraryTrack mlt)
        {
            _context = context;
            _logger = logger;
            _totalCount = count;
            _mlt = mlt;
        }

        public void Sync()
        {
            List<ISynchronizer> synchronizers = new List<ISynchronizer>();

            if (_mlt.albumArt.Any())
            {
                synchronizers.Add(new AlbumArtSynchronizer(_mlt, _context));
            }
            else
            {
                synchronizers.Add(new MainSynchonizer(_mlt, _context));
                //TODO Add the rest of the synchronizers...
            }

            foreach (ISynchronizer synchronizer in synchronizers)
            {
                synchronizer.Synchronize();
            }
        }
    }
}
