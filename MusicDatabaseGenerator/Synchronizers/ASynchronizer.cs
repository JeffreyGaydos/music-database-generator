using System;

namespace MusicDatabaseGenerator.Synchronizers
{
    public abstract class ASynchronizer
    {
        internal MusicLibraryTrack _mlt = null;
        internal static MusicLibraryContext _context;
        internal static LoggingUtils _logger;

        internal virtual SyncOperation Insert()
        {
            throw new NotImplementedException();
        }

        internal virtual SyncOperation Update()
        {
            throw new NotImplementedException();
        }

        public static SyncOperation Delete()
        {
            throw new NotImplementedException();
        }
    }
}
