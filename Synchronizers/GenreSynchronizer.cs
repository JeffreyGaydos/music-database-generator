using System;
using System.Collections.Generic;
using System.Linq;
/**
 * Data Dependecies: Main.TrackID (set by MainSynchronizer)
 */
namespace MusicDatabaseGenerator.Synchronizers
{
    public class GenreSynchronizer : ASynchronizer, ISynchronizer
    {
        public GenreSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context) {
            _context = context;
            _mlt = mlt;
        }

        public SyncOperation Synchronize()
        {
            return Insert();
        }

        internal override SyncOperation Insert()
        {
            List<string> currentGenres = _context.Genre.Select(g => g.GenreName).ToList();
            SyncOperation op = SyncOperation.None;

            foreach (string newGenre in _mlt.genre.Select(g => g.GenreName).Where(gn => !currentGenres.Contains(gn, new SQLStringComparison())).ToList())
            {
                List<string> newGenreList = new List<string> { newGenre };
                Genre fullGenre = _mlt.genre.Where(g => newGenreList.Contains(g.GenreName, new SQLStringComparison())).FirstOrDefault();
                if (fullGenre != null)
                {
                    _context.Genre.Add(fullGenre);
                    op |= SyncOperation.Insert;
                }
            }

            _context.SaveChanges();
            return op;
        }

        internal override SyncOperation Update()
        {
            return base.Update(); //Tracks are handled one at a time, so we can't be sure if there is an update until all tracks have run
        }

        public static new  SyncOperation Delete()
        {
            if(_context.Genre.Where(g => !_context.GenreTracks.Where(gt => gt.GenreID == g.GenreID).Any()).Any())
            {
                _context.Genre.RemoveRange(_context.Genre.Where(g => !_context.GenreTracks.Where(gt => gt.GenreID == g.GenreID).Any()));
                return SyncOperation.Delete;
            }
            return SyncOperation.None;
        }
    }
}
