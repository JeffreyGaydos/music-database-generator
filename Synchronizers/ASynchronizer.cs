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

        internal virtual void Insert()
        {
            throw new NotImplementedException();
        }

        internal virtual void Update()
        {
            throw new NotImplementedException();
        }
    }
}
