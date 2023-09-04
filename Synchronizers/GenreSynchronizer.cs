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
            Insert();
            return SyncOperation.Insert;
        }

        internal override void Insert()
        {
            List<string> currentGenres = _context.Genre.Select(g => g.GenreName).ToList();

            foreach (string newGenre in _mlt.genre.Select(g => g.GenreName).Where(gn => !currentGenres.Contains(gn, new SQLStringComparison())).ToList())
            {
                List<string> newGenreList = new List<string> { newGenre };
                Genre fullGenre = _mlt.genre.Where(g => newGenreList.Contains(g.GenreName, new SQLStringComparison())).FirstOrDefault();
                if (fullGenre != null)
                {
                    _context.Genre.Add(fullGenre);
                    _context.SaveChanges();
                }
            }

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
                _context.GenreTracks.Add(genreTrack);
            }
            _context.SaveChanges();
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
