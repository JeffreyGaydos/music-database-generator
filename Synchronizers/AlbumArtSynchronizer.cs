using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class AlbumArtSynchronizer : ASynchronizer, ISynchronizer
    {
        private Regex parentDirectoryMatch = new Regex(".*(?=[\\\\\\/][^\\\\\\/]*\\.[a-zA-Z]+)", RegexOptions.Compiled);

        public AlbumArtSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context)
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
            List<AlbumTracks> existingAlbumTracks = _context.AlbumTracks.ToList();
            foreach (AlbumArt art in _mlt.albumArt)
            {
                string parentDirectory = parentDirectoryMatch.Match(art.AlbumArtPath).Value;
                int trackID = _context.Main
                    .Where(m => m.FilePath.StartsWith(parentDirectory))
                    .Select(t => t.TrackID).FirstOrDefault();

                art.AlbumID = existingAlbumTracks.Where(at => at.TrackID == trackID).FirstOrDefault()?.AlbumID;
                _context.AlbumArt.Add(art);
            }
            _context.SaveChanges();
            return SyncOperation.Insert;
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
