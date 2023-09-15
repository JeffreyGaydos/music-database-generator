using System.Collections.Generic;
using System.Linq;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class PostProcessingSynchronizer : ASynchronizer, ISynchronizer
    {
        public PostProcessingSynchronizer(MusicLibraryContext context) {
            _context = context;
        }

        public SyncOperation Synchronize()
        {
            return Insert();
        }

        internal override SyncOperation Insert()
        {
            foreach (Artist a in _context.Artist)
            {
                a.PrimaryPersonID = _context.ArtistPersons.Where(ap => ap.ArtistID == a.ArtistID).FirstOrDefault()?.PersonID;
            }

            foreach (Album alb in _context.Album)
            {
                int albumYear = 0;
                List<int> albumTracks = _context.AlbumTracks.Where(at => at.AlbumID == alb.AlbumID).Select(a => a.TrackID).ToList();
                foreach (int TrackID in albumTracks)
                {
                    int trackYear = _context.Main.Where(m => m.TrackID == TrackID).FirstOrDefault()?.ReleaseYear ?? 0;
                    albumYear = trackYear > albumYear ? trackYear : albumYear;
                }
                alb.ReleaseYear = albumYear == 0 ? null : (int?)albumYear;
                alb.TrackCount = albumTracks.Count == 0 ? null : (int?)albumTracks.Count;
            }
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
