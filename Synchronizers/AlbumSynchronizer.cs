using System;
using System.Collections.Generic;
using System.Linq;
/**
 * Data Dependecies: Main.TrackID (set by MainSynchronizer)
 */
namespace MusicDatabaseGenerator.Synchronizers
{
    public class AlbumSynchronizer : ASynchronizer, ISynchronizer
    {
        public AlbumSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context, LoggingUtils logger)
        {
            _mlt = mlt;
            _context = context;
            _logger = logger;
        }

        public SyncOperation Synchronize()
        {
            return Insert();
        }

        internal override SyncOperation Insert()
        {
            List<string> currentAlbums = _context.Album.Select(a => a.AlbumName).ToList();
            SyncOperation op = SyncOperation.None;

            foreach (string newAlbum in _mlt.album.Select(a => a.Item1.AlbumName).Where(a => !currentAlbums.Contains(a, new SQLStringComparison())).ToList())
            {
                List<string> newAlbumList = new List<string> { newAlbum };
                Album fullAlbum = _mlt.album.Where(a => newAlbumList.Contains(a.Item1.AlbumName, new SQLStringComparison())).FirstOrDefault().Item1;
                if (fullAlbum != null)
                {
                    _context.Album.Add(fullAlbum);
                    op |= SyncOperation.Insert;
                }
            }

            _context.SaveChanges();
            return op;
        }

        internal override SyncOperation Update()
        {
            return base.Update();
        }

        public static new SyncOperation Delete()
        {
            if(_context.Album.Where(a => !_context.AlbumTracks.Where(at => at.AlbumID == a.AlbumID).Any()).Any())
            {
                _logger.GenerationLogWriteData($"Deleted {_context.Album.Where(a => !_context.AlbumTracks.Where(at => at.AlbumID == a.AlbumID).Any()).Count()} entries from Album");
                _context.Album.RemoveRange(_context.Album.Where(a => !_context.AlbumTracks.Where(at => at.AlbumID == a.AlbumID).Any()));
                return SyncOperation.Delete;
            }
            return SyncOperation.None;
        }
    }
}
