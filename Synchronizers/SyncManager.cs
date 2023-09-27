using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class SyncManager
    {
        private static MusicLibraryContext _context;
        private LoggingUtils _logger;
        private int _totalCount;
        private MusicLibraryTrack _mlt;
        private List<ISynchronizer> _synchronizers = new List<ISynchronizer>();

        private static int trackIndex = 0;

        private static Regex R_PKViolationTable = new Regex("dbo\\.[a-zA-Z]+", RegexOptions.Compiled);
        private static Regex R_PKViolationViolatingKey = new Regex("(?<=\\().+(?=\\))", RegexOptions.Compiled);

        public SyncManager(MusicLibraryContext context, LoggingUtils logger, int count, MusicLibraryTrack mlt)
        {
            _context = context;
            _logger = logger;
            _totalCount = count;
            _mlt = mlt;
        }

        public void Sync()
        {
            trackIndex++;
            if (_mlt.albumArt.Any())
            {
                _synchronizers.Add(new AlbumArtSynchronizer(_mlt, _context));
            }
            else
            {   //order matters...
                _synchronizers.Add(new MainSynchonizer(_mlt, _context));
                _synchronizers.Add(new GenreSynchronizer(_mlt, _context));
                _synchronizers.Add(new GenreTrackSynchronizer(_mlt, _context));
                _synchronizers.Add(new ArtistSynchronizer(_mlt, _context));
                _synchronizers.Add(new ArtistTrackSynchronizer(_mlt, _context));
                _synchronizers.Add(new AlbumSynchronizer(_mlt, _context));
                _synchronizers.Add(new AlbumTrackSynchronizer(_mlt, _context));
                _synchronizers.Add(new ArtistPersonsSynchronizer(_mlt, _context));
                _synchronizers.Add(new TrackPersonsSynchronizer(_mlt, _context));

                _synchronizers.Add(new PostProcessingSynchronizer(_context));
            }

            SyncOperation ops = SyncOperation.None;

            using (DbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (ISynchronizer synchronizer in _synchronizers)
                    {
                        ops |= synchronizer.Synchronize();
                        if((ops & SyncOperation.Skip) > 0)
                        {
                            _logger.GenerationLogWriteData($"{100 * (trackIndex) / (decimal)_totalCount:00.00}% Finished processing track {trackIndex} DUPLICATE (skipped) ({_mlt.main.Title})");
                            return; //skip the title
                        }
                    }

                    transaction.Commit();
                }
                catch (DbUpdateException ue)
                {
                    string innerMessage = ue.InnerException.InnerException.Message;
                    if (innerMessage.Contains("Violation of UNIQUE KEY constraint"))
                    {
                        //This should be deprecated with the new update stuff...
                        string key = R_PKViolationViolatingKey.Match(innerMessage).Value;
                        string table = R_PKViolationTable.Match(innerMessage).Value;
                        //_logger.GenerationLogWriteData($"{percentageString} Finished processing track {trackIndex} DUPLICATE (skipped) ({_mlt.main.Title})");
                        _logger.DuplicateLogWriteData($"{_mlt.main.FilePath}: UNIQUE constraint violation on table '{table}' from key: [{key}]");
                        transaction.Rollback();
                        //The first entry in the change tracker is the most recent change, i.e. the thing that threw an exception. We need to remove it from the context
                        _context.ChangeTracker.Entries().First().State = EntityState.Detached;
                    } else
                    {
                        throw ue; //we don't actually know what's going on then...
                    }
                }
            }
            string percentageString = $"{100 * trackIndex / (decimal)_totalCount:00.00}%";
            LogOperation(ops, percentageString);
        }

        public static void Delete()
        {
            SyncOperation ops = SyncOperation.None;

            using (DbContextTransaction transaction = _context.Database.BeginTransaction())
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
                transaction.Commit();
            }
        }

        private void LogOperation(SyncOperation ops, string percentageString)
        {
            switch (ops)
            {
                case SyncOperation.None:
                    _logger.GenerationLogWriteData($"{percentageString} No changes found for track {_mlt.main.Title} ({_mlt.main.TrackID})");
                    break;
                default:
                    _logger.GenerationLogWriteData($"{percentageString} Finished opertaions {ops} on track {trackIndex} ({_mlt.main.Title})");
                    break;
            }
        }
    }
}
