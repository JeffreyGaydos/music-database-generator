using System;
using System.Collections.Generic;
using System.Linq;
/**
 * Data Dependecies: Main.TrackID (set by MainSynchronizer)
 */
namespace MusicDatabaseGenerator.Synchronizers
{
    public class ArtistSynchronizer : ASynchronizer, ISynchronizer
    {
        public ArtistSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context) {
            _mlt = mlt;
            _context = context;
        }

        public SyncOperation Synchronize()
        {
            return Insert();
        }

        internal override SyncOperation Insert()
        {
            List<string> currentArtists = _context.Artist.Select(a => a.ArtistName).ToList();
            SyncOperation op = SyncOperation.None;

            foreach (string newArtist in _mlt.artist.Select(a => a.ArtistName).Where(a => !currentArtists.Contains(a, new SQLStringComparison())).ToList())
            {
                List<string> newArtistList = new List<string> { newArtist };
                Artist fullArtist = _mlt.artist.Where(a => newArtistList.Contains(a.ArtistName, new SQLStringComparison())).FirstOrDefault();
                if (fullArtist != null)
                {
                    _context.Artist.Add(fullArtist);
                    _context.SaveChanges();
                    op |= SyncOperation.Insert;
                }
            }

            _context.SaveChanges();
            return op;
        }

        internal override SyncOperation Update()
        {
            return Insert(); //putting this here to designate that the insert function operates like an update function
        }

        public static new SyncOperation Delete()
        {
            if(_context.Artist.Where(a => !_context.ArtistTracks.Where(at => at.ArtistID == a.ArtistID).Any()).Any())
            {
                _context.Artist.RemoveRange(_context.Artist.Where(a => !_context.ArtistTracks.Where(at => at.ArtistID == a.ArtistID).Any()));
                return SyncOperation.Delete;
            }
            return SyncOperation.None;
        }
    }
}
