using System.Linq;
/**
 * Data Dependecies: Main.TrackID (set by MainSynchronizer)
 */
namespace MusicDatabaseGenerator.Synchronizers
{
    public class TrackPersonsSynchronizer : ASynchronizer, ISynchronizer
    {
        public TrackPersonsSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context)
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
            SyncOperation op = SyncOperation.None;

            foreach ((TrackPersons tp, string name) person in _mlt.trackPersons)
            {
                person.tp.PersonID = _context.ArtistPersons.Where(ap => ap.PersonName == person.name).Select(a => a.PersonID).FirstOrDefault();
                person.tp.TrackID = _mlt.main.TrackID;
                if (person.tp.PersonID != 0 && !_context.TrackPersons.Where(db => db.TrackID == person.tp.TrackID && db.PersonID == person.tp.PersonID).Any())
                {
                    _context.TrackPersons.Add(person.tp);
                    op |= SyncOperation.Insert;
                }
            }
            _context.SaveChanges();
            return op;
        }

        internal override SyncOperation Update()
        {
            return base.Update(); //N/A Track Persons has immutable data only
        }

        public static new SyncOperation Delete()
        {
            if(_context.TrackPersons.Where(tp => !_context.Main.Where(m => m.TrackID == tp.TrackID).Any()).Any())
            {
                _context.TrackPersons.RemoveRange(_context.TrackPersons.Where(tp => !_context.Main.Where(m => m.TrackID == tp.TrackID).Any()));
                return SyncOperation.Delete;
            }
            return SyncOperation.None;
        }
    }
}
