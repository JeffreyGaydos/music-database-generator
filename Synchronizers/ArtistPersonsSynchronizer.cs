using System;
using System.Linq;
/**
 * Data Dependecies: Artist (set by ArtistSynchronizer)
 */
namespace MusicDatabaseGenerator.Synchronizers
{
    public class ArtistPersonsSynchronizer : ASynchronizer, ISynchronizer
    {
        public ArtistPersonsSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context, LoggingUtils logger)
        {
            _mlt = mlt;
            _context = context;
            _logger = logger;
        }

        public SyncOperation Synchronize()
        {
            return Insert();
        }

        internal override SyncOperation Insert()
        {
            SyncOperation op = SyncOperation.None;
            foreach (ArtistPersons person in _mlt.artistPersons)
            {
                //If there are > 1 artists on 1 track, the standard mp3 metadata on performers is not very useful
                //and likely will not contain the information needed to map performers to specific artists. In this
                //case, the artistID that we map here may be inaccurate or relate to a different artist on the same
                //track
                person.ArtistID = _mlt.artist.Select(a => a.ArtistID).FirstOrDefault();
                if (!_context.ArtistPersons.Select(p => p.PersonName).Contains(person.PersonName))
                {
                    _context.ArtistPersons.Add(person);
                    op |= SyncOperation.Insert;
                }
            }
            _context.SaveChanges();
            return op;
        }

        internal override SyncOperation Update()
        {
            return Insert();
        }

        public static new SyncOperation Delete()
        {
            if(_context.ArtistPersons.Where(ap => !_context.Artist.Where(a => a.ArtistID == ap.ArtistID).Any()).Any())
            {
                _logger.GenerationLogWriteData($"Deleted {_context.ArtistPersons.Where(ap => !_context.Artist.Where(a => a.ArtistID == ap.ArtistID).Any()).Count()} entries from ArtistPersons");
                _context.ArtistPersons.RemoveRange(_context.ArtistPersons.Where(ap => !_context.Artist.Where(a => a.ArtistID == ap.ArtistID).Any()));
                return SyncOperation.Delete;
            }
            return SyncOperation.None;
        }
    }
}
