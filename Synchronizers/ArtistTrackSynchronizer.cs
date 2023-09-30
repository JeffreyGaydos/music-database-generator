using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class ArtistTrackSynchronizer : ASynchronizer, ISynchronizer
    {
        public ArtistTrackSynchronizer(MusicLibraryTrack mlt, MusicLibraryContext context, LoggingUtils logger)
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
            SyncOperation op = SyncOperation.None;

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
                //PK check
                if (!_context.ArtistTracks.Where(db => db.TrackID == artistTrack.TrackID && db.ArtistID == artistTrack.ArtistID).Any())
                {
                    _context.ArtistTracks.Add(artistTrack);
                    op |= SyncOperation.Insert;
                }
            }

            _context.SaveChanges();
            return op;
        }

        internal override SyncOperation Update()
        {
            return base.Update(); //N/A: Artist Tracks are mappings between 2 fields that are immutable (IDs)
        }

        public static new SyncOperation Delete()
        {
            if(_context.ArtistTracks.Where(at => _context.Main.Where(m => m.TrackID == at.TrackID).Any()).Any())
            {
                _logger.GenerationLogWriteData($"Deleted {_context.ArtistTracks.Where(at => _context.Main.Where(m => m.TrackID == at.TrackID).Any()).Count()} entries from ArtistTracks");
                _context.ArtistTracks.RemoveRange(_context.ArtistTracks.Where(at => _context.Main.Where(m => m.TrackID == at.TrackID).Any()));
                return SyncOperation.Delete;
            }
            return SyncOperation.None;
        }
    }
}
