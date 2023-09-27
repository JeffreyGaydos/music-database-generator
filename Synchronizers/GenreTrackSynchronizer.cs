using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class GenreTrackSynchronizer : ASynchronizer, ISynchronizer
    {
        public GenreTrackSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context)
        {
            _context = context;
            _mlt = mlt;
        }

        public SyncOperation Synchronize()
        {
            return Insert();
        }

        internal override SyncOperation Insert()
        {
            SyncOperation op = SyncOperation.None;

            foreach (Genre g in _mlt.genre)
            {
                g.GenreID = _context.Genre.ToList()
                        .Where(dbg => {
                            List<string> list = new List<string> { dbg.GenreName };
                            return list.Contains(g.GenreName, new SQLStringComparison());
                        })
                        .FirstOrDefault().GenreID;

                GenreTracks genreTrack = new GenreTracks();
                genreTrack.TrackID = _mlt.main.TrackID;
                genreTrack.GenreID = g.GenreID;
                if(!_context.GenreTracks.Where(db => db.TrackID == genreTrack.TrackID && db.GenreID == genreTrack.GenreID).Any())
                {
                    _context.GenreTracks.Add(genreTrack);
                    op |= SyncOperation.Insert;
                }
            }

            _context.SaveChanges();
            return op;
        }

        internal override SyncOperation Update()
        {
            return base.Update(); //N/A Genre Tracks have immutable data only
        }

        public static new SyncOperation Delete()
        {
            if(_context.GenreTracks.Where(gt => !_context.Main.Select(m => m.TrackID == gt.TrackID).Any()).Any())
            {
                _context.GenreTracks.RemoveRange(_context.GenreTracks.Where(gt => !_context.Main.Select(m => m.TrackID == gt.TrackID).Any()));
                return SyncOperation.Delete;
            }
            return SyncOperation.None;
        }
    }
}
