using System;
using System.Linq;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class MainSynchonizer : ASynchronizer, ISynchronizer
    {
        public MainSynchonizer(MusicLibraryTrack mlt, MusicLibraryContext context) {
            _mlt = mlt;
            _context = context;
        }

        public void Synchronize()
        {
            Insert();
        }

        internal override void Insert()
        {
            _mlt.main.GeneratedDate = DateTime.Now;
            _context.Main.Add(_mlt.main);
            _context.SaveChanges();
            int? trackID = _context.Main.ToList().Where(m => m == _mlt.main).FirstOrDefault()?.TrackID;
            if (trackID == null)
            {
                throw new Exception($"Could not create a Main record for track {_mlt.main.Title}");
            }
            _mlt.main.TrackID = trackID.Value;
        }

        internal override void Update()
        {
            base.Update();
        }

        internal override void Delete()
        {
            base.Delete();
        }
    }
}
