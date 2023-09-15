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
            foreach ((TrackPersons tp, string name) person in _mlt.trackPersons)
            {
                person.tp.PersonID = _context.ArtistPersons.Where(ap => ap.PersonName == person.name).Select(a => a.PersonID).FirstOrDefault();
                person.tp.TrackID = _mlt.main.TrackID;
                if (person.tp.PersonID != 0)
                    _context.TrackPersons.Add(person.tp);
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
