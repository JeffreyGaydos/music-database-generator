using System;

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
            base.Insert();
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
