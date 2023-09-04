using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class AlbumArtSynchronizer : ASynchronizer, ISynchronizer
    {
        public AlbumArtSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context)
        {
            _context = context;
            _mlt = mlt;
        }

        public void Synchronize()
        {
            
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
