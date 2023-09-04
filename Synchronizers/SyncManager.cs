using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Data.Entity.Infrastructure.Design.Executor;
using System;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class SyncManager
    {
        private MusicLibraryContext _context;
        private LoggingUtils _logger;
        private int _totalCount;
        private MusicLibraryTrack _mlt;

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
            List<ISynchronizer> synchronizers = new List<ISynchronizer>();

            if (_mlt.albumArt.Any())
            {
                synchronizers.Add(new AlbumArtSynchronizer(_mlt, _context));
            }
            else
            {   //order matters...
                synchronizers.Add(new MainSynchonizer(_mlt, _context));
                synchronizers.Add(new GenreSynchronizer(_mlt, _context));
                synchronizers.Add(new ArtistSynchronizer(_mlt, _context));
                synchronizers.Add(new AlbumSynchronizer(_mlt, _context));
                synchronizers.Add(new ArtistPersonsSynchronizer(_mlt, _context));
                synchronizers.Add(new TrackPersonsSynchronizer(_mlt, _context));

                synchronizers.Add(new PostProcessingSynchronizer(_context));                
            }

            using (DbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                string percentageString = $"{100 * trackIndex / (decimal)_totalCount:00.00}%";
                SyncOperation ops = SyncOperation.None;
                try
                {
                    foreach (ISynchronizer synchronizer in synchronizers)
                    {
                        ops |= synchronizer.Synchronize();
                    }

                    transaction.Commit();

                    switch(ops)
                    {
                        case SyncOperation.None:
                            _logger.GenerationLogWriteData($"{percentageString} No updates found for track {_mlt.main.Title} ({_mlt.main.TrackID})");
                            break;
                        default:
                            _logger.GenerationLogWriteData($"{percentageString} Finished opertaions {ops} on track {trackIndex} ({_mlt.main.Title})");
                            break;
                    }
                }
                catch (DbUpdateException ue)
                {
                    string innerMessage = ue.InnerException.InnerException.Message;
                    if (innerMessage.Contains("Violation of UNIQUE KEY constraint"))
                    {
                        string key = R_PKViolationViolatingKey.Match(innerMessage).Value;
                        string table = R_PKViolationTable.Match(innerMessage).Value;
                        _logger.GenerationLogWriteData($"{percentageString} Finished processing track {trackIndex} DUPLICATE (skipped) ({_mlt.main.Title})");
                        _logger.DuplicateLogWriteData($"{_mlt.main.FilePath}: UNIQUE constraint violation on table '{table}' from key: [{key}]");
                        transaction.Rollback();
                        //The first entry in the change tracker is the most recent change, i.e. the thing that threw an exception. We need to remove it from the context
                        _context.ChangeTracker.Entries().First().State = EntityState.Detached;
                    }
                }
            }
        }
    }
}
