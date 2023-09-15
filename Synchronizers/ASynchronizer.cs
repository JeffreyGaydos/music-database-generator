using System;

namespace MusicDatabaseGenerator.Synchronizers
{
    public abstract class ASynchronizer
    {
        internal MusicLibraryTrack _mlt = null;
        internal MusicLibraryContext _context;

        internal virtual void Delete()
        {
            throw new NotImplementedException();
        }

        internal virtual SyncOperation Insert()
        {
            throw new NotImplementedException();
        }

        internal virtual SyncOperation Update()
        {
            throw new NotImplementedException();
        }
    }
}
