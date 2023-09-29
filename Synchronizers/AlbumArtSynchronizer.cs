using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class AlbumArtSynchronizer : ASynchronizer, ISynchronizer
    {
        private Regex parentDirectoryMatch = new Regex(".*(?=[\\\\\\/][^\\\\\\/]*\\.[a-zA-Z]+)", RegexOptions.Compiled);

        public AlbumArtSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context, LoggingUtils logger)
        {
            _context = context;
            _mlt = mlt;
            _logger = logger;
        }

        public SyncOperation Synchronize()
        {
            if(_context.AlbumArt.Where(aa => aa.AlbumArtPath == _mlt.albumArt.AlbumArtPath).Any())
            {
                return Update();
            } else
            {
                return Insert();
            }
        }

        internal override SyncOperation Insert()
        {
            List<AlbumTracks> existingAlbumTracks = _context.AlbumTracks.ToList();
            
            string parentDirectory = parentDirectoryMatch.Match(_mlt.albumArt.AlbumArtPath).Value;
            int trackID = _context.Main
                .Where(m => m.FilePath.StartsWith(parentDirectory))
                .Select(t => t.TrackID).FirstOrDefault();

            _mlt.albumArt.AlbumID = existingAlbumTracks.Where(at => at.TrackID == trackID).FirstOrDefault()?.AlbumID;
            _context.AlbumArt.Add(_mlt.albumArt);

            _context.SaveChanges();
            return SyncOperation.Insert;
        }

        internal override SyncOperation Update()
        {
            AlbumArt match = _context.AlbumArt.Where(aa => aa.AlbumArtPath == _mlt.albumArt.AlbumArtPath).FirstOrDefault();
            if(_mlt.albumArt.PrimaryColor != match.PrimaryColor)
            {
                _logger.GenerationLogWriteData($"Updated Primary color for Album art associated with AlbumID {match.AlbumID} from {match.PrimaryColor} to {_mlt.albumArt.PrimaryColor}", true);
                match.PrimaryColor = _mlt.albumArt.PrimaryColor;
                _context.SaveChanges();
                return SyncOperation.Update;
            }
            return SyncOperation.Skip;
        }

        public static new SyncOperation Delete()
        {
            if(_context.AlbumArt.Where(aa => !_context.Album.Where(a => a.AlbumID == aa.AlbumID).Any()).Any())
            {
                _logger.GenerationLogWriteData($"Deleted {_context.AlbumArt.Where(aa => !_context.Album.Where(a => a.AlbumID == aa.AlbumID).Any()).Count()} entries from AlbumArt");
                _context.AlbumArt.RemoveRange(_context.AlbumArt.Where(aa => !_context.Album.Where(a => a.AlbumID == aa.AlbumID).Any()));
            }
            return SyncOperation.None;
        }
    }
}
