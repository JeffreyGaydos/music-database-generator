using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class AlbumTrackSynchronizer : ASynchronizer, ISynchronizer
    {
        public AlbumTrackSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context)
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

            foreach ((Album, AlbumTracks) a in _mlt.album)
            {
                a.Item1.AlbumID = _context.Album.ToList()
                        .Where(dba => {
                            List<string> list = new List<string> { dba.AlbumName };
                            return list.Contains(a.Item1.AlbumName, new SQLStringComparison());
                        })
                        .FirstOrDefault().AlbumID;

                a.Item2.AlbumID = a.Item1.AlbumID;
                a.Item2.TrackID = _mlt.main.TrackID;
                if(!_context.AlbumTracks.Where(db => db.TrackID == a.Item2.TrackID && db.AlbumID == a.Item2.AlbumID).Any())
                {
                    _context.AlbumTracks.Add(a.Item2);
                    op |= SyncOperation.Insert;
                }
            }

            _context.SaveChanges();
            return op;
        }

        internal override SyncOperation Update()
        {
            return base.Update(); //N/A Album Tracks only have immutable data in them
        }

        public SyncOperation Delete()
        {
            return SyncOperation.None;
        }
    }
}
