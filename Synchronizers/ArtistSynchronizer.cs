using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Insert();
            return SyncOperation.Insert;
        }

        internal override void Insert()
        {
            List<string> currentArtists = _context.Artist.Select(a => a.ArtistName).ToList();

            foreach (string newArtist in _mlt.artist.Select(a => a.ArtistName).Where(a => !currentArtists.Contains(a, new SQLStringComparison())).ToList())
            {
                List<string> newArtistList = new List<string> { newArtist };
                Artist fullArtist = _mlt.artist.Where(a => newArtistList.Contains(a.ArtistName, new SQLStringComparison())).FirstOrDefault();
                if (fullArtist != null)
                {
                    _context.Artist.Add(fullArtist);
                    _context.SaveChanges();
                }
            }

            foreach (Artist a in _mlt.artist)
            {
                a.ArtistID = _context.Artist.ToList()
                        .Where(dba => {
                            List<string> list = new List<string> { dba.ArtistName };
                            return list.Contains(a.ArtistName, new SQLStringComparison());
                        })
                        .FirstOrDefault().ArtistID;

                ArtistTracks artistTrack = new ArtistTracks();
                artistTrack.TrackID = _mlt.main.TrackID;
                artistTrack.ArtistID = a.ArtistID;
                _context.ArtistTracks.Add(artistTrack);
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
