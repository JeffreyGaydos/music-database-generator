using MusicDatabaseGenerator;
using MusicDatabaseGenerator.Synchronizers;
using System.Collections.Generic;
using System.Linq;

namespace PlaylistTransferTool.Synchronizers
{
    public class PlaylistTrackSynchronizer
    {
        private readonly List<PlaylistTracks> _playlistTracks;
        private readonly MusicLibraryContext _context;

        public PlaylistTrackSynchronizer(List<PlaylistTracks> playlistTracks, MusicLibraryContext context)
        {
            _playlistTracks = playlistTracks;
            _context = context;
        }

        public SyncOperation Sync()
        {
            LoggingUtils.GenerationLogWriteData($"Applying changes to database...");
            SyncOperation op = SyncOperation.None;
            foreach(var plt in _playlistTracks)
            {
                if(_context.PlaylistTracks.Where(pt => pt.TrackOrder == plt.TrackOrder && pt.PlaylistID == plt.PlaylistID).Any())
                {
                    var currentPt = _context.PlaylistTracks.FirstOrDefault(pt => pt.TrackOrder == plt.TrackOrder && pt.PlaylistID == plt.PlaylistID);
                    if (currentPt.LastKnownPath != plt.LastKnownPath || currentPt.TrackID != plt.TrackID)
                    {
                        op |= SyncOperation.Update;
                        LoggingUtils.GenerationLogWriteData($"Track with with path '{plt.LastKnownPath}' is already in this playlist but has changes, updating");
                        //since our PK uses the track ID, we need to delete and re-insert rather than an in-place update

                        _context.PlaylistTracks.Remove(currentPt);
                        _context.SaveChanges();
                        if (_context.PlaylistTracks.Any(ept => ept.PlaylistID == plt.PlaylistID && ept.TrackID == plt.TrackID && ept.TrackID.HasValue))
                        {
                            LoggingUtils.GenerationLogWriteDataSameLine($"Removed Duplciate TrackID {plt.LastKnownPath}");
                            _context.PlaylistTracks.Remove(_context.PlaylistTracks.First(ept => ept.PlaylistID == plt.PlaylistID && ept.TrackID == plt.TrackID));
                            _context.SaveChanges();
                        }
                        _context.PlaylistTracks.Add(plt);
                        _context.SaveChanges();
                    } else
                    {
                        op |= SyncOperation.Skip;
                        LoggingUtils.GenerationLogWriteData($"Track with path '{plt.LastKnownPath}' is already in this playlist and has no changes, skipping");
                    }
                } else if (_context.PlaylistTracks.Where(pt => plt.TrackID.HasValue && pt.TrackID == plt.TrackID && pt.PlaylistID == plt.PlaylistID).Any())
                {
                    var currentPt = _context.PlaylistTracks.FirstOrDefault(pt => pt.TrackID == plt.TrackID && pt.PlaylistID == plt.PlaylistID);
                    if (currentPt.LastKnownPath != plt.LastKnownPath || currentPt.TrackOrder != plt.TrackOrder)
                    {
                        op |= SyncOperation.Update;
                        LoggingUtils.GenerationLogWriteData($"Track with path '{plt.LastKnownPath}' is already in this playlist but has changes, updating");
                        //since our PK uses the track ID, we need to delete and re-insert rather than an in-place update

                        _context.PlaylistTracks.Remove(currentPt);
                        _context.SaveChanges();
                        if (_context.PlaylistTracks.Any(ept => ept.PlaylistID == plt.PlaylistID && ept.TrackOrder == plt.TrackOrder && ept.TrackOrder != null))
                        {
                            LoggingUtils.GenerationLogWriteDataSameLine($"Removed Duplciate Track Order {plt.LastKnownPath}");
                            _context.PlaylistTracks.Remove(_context.PlaylistTracks.First(ept => ept.PlaylistID == plt.PlaylistID && ept.TrackOrder == plt.TrackOrder));
                            _context.SaveChanges();
                        }
                        _context.PlaylistTracks.Add(plt);
                        _context.SaveChanges();
                    }
                    else
                    {
                        op |= SyncOperation.Skip;
                        LoggingUtils.GenerationLogWriteData($"Track with path '{plt.LastKnownPath}' is already in this playlist and has no changes, skipping");
                    }
                } else
                {
                    op |= SyncOperation.Insert;
                    if(_context.PlaylistTracks.Any(pt => pt.TrackID.HasValue && pt.TrackID == plt.TrackID && pt.PlaylistID == plt.PlaylistID) || _context.PlaylistTracks.Any(pt => pt.TrackOrder.HasValue && pt.TrackOrder == plt.TrackOrder && pt.PlaylistID == plt.PlaylistID))
                    {
                        LoggingUtils.GenerationLogWriteData($"Track with ID {plt.TrackID} from {plt.LastKnownPath} wanted to be inserted but");
                    } else
                    {
                        _context.PlaylistTracks.Add(plt);
                        _context.SaveChanges();
                    }
                }
            }

            _context.SaveChanges();

            return op;
        }
    }
}
