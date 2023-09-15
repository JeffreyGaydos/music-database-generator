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
        public AlbumSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context)
        {
            _mlt = mlt;
            _context = context;
        }

        public SyncOperation Synchronize()
        {
            return Insert();
        }

        internal override SyncOperation Insert()
        {
            List<string> currentAlbums = _context.Album.Select(a => a.AlbumName).ToList();

            foreach (string newAlbum in _mlt.album.Select(a => a.Item1.AlbumName).Where(a => !currentAlbums.Contains(a, new SQLStringComparison())).ToList())
            {
                List<string> newAlbumList = new List<string> { newAlbum };
                Album fullAlbum = _mlt.album.Where(a => newAlbumList.Contains(a.Item1.AlbumName, new SQLStringComparison())).FirstOrDefault().Item1;
                if (fullAlbum != null)
                {
                    _context.Album.Add(fullAlbum);
                    _context.SaveChanges();
                }
            }

            foreach ((Album, AlbumTracks) a in _mlt.album)
            {
                a.Item1.AlbumID = _context.Album.ToList()
                        .Where(dba => {
                            List<string> list = new List<string> { dba.AlbumName };
                            return list.Contains(a.Item1.AlbumName, new SQLStringComparison());
                        })
                        .FirstOrDefault().AlbumID;

                a.Item2.AlbumID = a.Item1.AlbumID;
                a.Item2.TrackID = _mlt.main.TrackID;
                _context.AlbumTracks.Add(a.Item2);
            }
            _context.SaveChanges();
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
