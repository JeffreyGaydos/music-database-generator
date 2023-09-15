using System;
using System.Linq;
/**
 * Data Dependecies: Artist (set by ArtistSynchronizer)
 */
namespace MusicDatabaseGenerator.Synchronizers
{
    public class ArtistPersonsSynchronizer : ASynchronizer, ISynchronizer
    {
        public ArtistPersonsSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context)
        {
            _mlt = mlt;
            _context = context;
        }

        public SyncOperation Synchronize()
        {
            return Insert();
        }

        internal override SyncOperation Insert()
        {
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
                }
            }
            _context.SaveChanges();
            return SyncOperation.Insert;
        }

        internal override SyncOperation Update()
        {
            return base.Update();
        }

        internal override void Delete()
        {
            base.Delete();
        }
    }
}
