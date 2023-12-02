using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class SyncManager
    {
        private static MusicLibraryContext _context;
        private LoggingUtils _logger;
        private int _totalCount;
        private MusicLibraryTrack _mlt;
        private List<ISynchronizer> _synchronizers = new List<ISynchronizer>();
        
        private static bool albumArtSync = false;
        public static int Inserts = 0;
        public static int Updates = 0;
        public static int Skips = 0;

        public SyncManager(MusicLibraryContext context, LoggingUtils logger, int count, MusicLibraryTrack mlt, bool in_albumArtSync)
        {
            _context = context;
            _logger = logger;
            _totalCount = count;
            _mlt = mlt;
            albumArtSync = in_albumArtSync;
            _stopwatch = new Stopwatch();
        }

        public void Sync()
        {
            if (albumArtSync)
            {
                _synchronizers.Add(new AlbumArtSynchronizer(_mlt, _context, _logger));
            }
            else
            {   //order matters...
                _synchronizers.Add(new MainSynchonizer(_mlt, _context, _logger));
                _synchronizers.Add(new GenreSynchronizer(_mlt, _context, _logger));
                _synchronizers.Add(new GenreTrackSynchronizer(_mlt, _context, _logger));
                _synchronizers.Add(new ArtistSynchronizer(_mlt, _context, _logger));
                _synchronizers.Add(new ArtistTrackSynchronizer(_mlt, _context, _logger));
                _synchronizers.Add(new AlbumSynchronizer(_mlt, _context, _logger));
                _synchronizers.Add(new AlbumTrackSynchronizer(_mlt, _context, _logger));
                _synchronizers.Add(new ArtistPersonsSynchronizer(_mlt, _context, _logger));
                _synchronizers.Add(new TrackPersonsSynchronizer(_mlt, _context, _logger));

                _synchronizers.Add(new PostProcessingSynchronizer(_context));
            }

            SyncOperation ops = SyncOperation.None;
            string percentageString = $"{100 * (albumArtSync ? MusicLibraryTrack.albumArtIndex : MusicLibraryTrack.trackIndex) / (decimal)_totalCount:00.00}% | ";
            using (DbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                foreach (ISynchronizer synchronizer in _synchronizers)
                {
                    ops |= synchronizer.Synchronize();
                    UpdateLogVariables(ops);
                    if ((ops & SyncOperation.Skip) > 0)
                    {
                        if(albumArtSync)
                        {
                            _logger.GenerationLogWriteData($"{percentageString} Finished processing album art {MusicLibraryTrack.albumArtIndex} (skipped) ({_mlt.albumArt.AlbumArtPath})");
                        } else
                        {
                            _logger.GenerationLogWriteData($"{percentageString} Finished processing track {(albumArtSync ? MusicLibraryTrack.albumArtIndex : MusicLibraryTrack.trackIndex)} (skipped) ({_mlt.main.Title})");
                        }
                        return; //skip the title
                    }
                }

                transaction.Commit();
            }
            LogOperation(ops, percentageString);
        }

        public static void Delete()
        {
            SyncOperation ops = SyncOperation.None;

            using (DbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                if (albumArtSync)
                {
                    ops |= AlbumArtSynchronizer.Delete();
                } else
                {
                    // order matters!
                    ops |= MainSynchonizer.Delete();
                    ops |= GenreTrackSynchronizer.Delete();
                    ops |= GenreSynchronizer.Delete();
                    ops |= ArtistTrackSynchronizer.Delete();
                    ops |= ArtistSynchronizer.Delete();
                    ops |= AlbumTrackSynchronizer.Delete();
                    ops |= AlbumSynchronizer.Delete();
                    ops |= ArtistPersonsSynchronizer.Delete();
                    ops |= TrackPersonsSynchronizer.Delete();
                    ops |= MoodTrackSynchronizer.Delete();
                    ops |= PlaylistTrackSynchronizer.Delete();
                }
                transaction.Commit();
            }
        }

        private void LogOperation(SyncOperation ops, string percentageString)
        {
            switch (ops)
            {
                case SyncOperation.None:
                    if(albumArtSync)
                    {
                        _logger.GenerationLogWriteData($"{percentageString} No changes found for album art {(albumArtSync ? MusicLibraryTrack.albumArtIndex : MusicLibraryTrack.trackIndex)} ({_mlt.albumArt.AlbumArtPath})");
                    } else
                    {
                        _logger.GenerationLogWriteData($"{percentageString} No changes found for track {_mlt.main.Title} ({_mlt.main.TrackID})");
                    }
                    break;
                default:
                    if(albumArtSync)
                    {
                        _logger.GenerationLogWriteData($"{percentageString} Finished opertaions {ops} on album art {(albumArtSync ? MusicLibraryTrack.albumArtIndex : MusicLibraryTrack.trackIndex)} ({_mlt.albumArt.AlbumArtPath})");
                    } else
                    {
                        _logger.GenerationLogWriteData($"{percentageString} Finished opertaions {ops} on track {(albumArtSync ? MusicLibraryTrack.albumArtIndex : MusicLibraryTrack.trackIndex)} ({_mlt.main.Title})");
                    }
                    break;
            }
        }

        private void UpdateLogVariables(SyncOperation ops)
        {
            if((ops & SyncOperation.Skip) > 0)
            {
                Skips++;
            }
            if((ops & SyncOperation.Update) > 0)
            {
                Updates++;
            }
            if((ops & SyncOperation.Insert) > 0)
            {
                Inserts++;
            }
        }
    }
}
