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
                int? PrimaryPersonID = _context.ArtistPersons.Where(ap => ap.ArtistID == a.ArtistID).FirstOrDefault()?.PersonID;
                if(a.PrimaryPersonID != PrimaryPersonID)
                {
                    a.PrimaryPersonID = _context.ArtistPersons.Where(ap => ap.ArtistID == a.ArtistID).FirstOrDefault()?.PersonID;
                }
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
                int? ReleaseYear = albumYear == 0 ? null : (int?)albumYear;
                int? TrackCount = albumTracks.Count == 0 ? null : (int?)albumTracks.Count;
                if(alb.ReleaseYear != ReleaseYear || alb.TrackCount != TrackCount)
                {
                    alb.ReleaseYear = ReleaseYear;
                    alb.TrackCount = TrackCount;
                }
            }
            return SyncOperation.None; //The operation type is implied from previous synchronizers; return default
        }

        internal override SyncOperation Update()
        {
            return base.Update();
        }

        public SyncOperation Delete()
        {
            return SyncOperation.None;
        }
    }
}
