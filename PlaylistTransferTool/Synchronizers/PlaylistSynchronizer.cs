using MusicDatabaseGenerator;
using MusicDatabaseGenerator.Synchronizers;
using System.Linq;

namespace PlaylistTransferTool.Synchronizers
{
    public class PlaylistSynchronizer
    {
        private Playlist _playlist;
        private MusicLibraryContext _context;
        private ConfiguratorValues _config;

        public PlaylistSynchronizer(Playlist playlist, MusicLibraryContext context, ConfiguratorValues config)
        {
            _playlist = playlist;
            _context = context;
            _config = config;
        }

        public SyncOperation Sync()
        {
            if(_context.Playlist.Where(p => p.PlaylistName == _playlist.PlaylistName).Any())
            {
                if (_config.MergePlaylistsWithSameName)
                {
                    _playlist.PlaylistID = _context.Playlist.Where(
                        p => p.PlaylistName == _playlist.PlaylistName
                    ).FirstOrDefault().PlaylistID;

                    if(_context.Playlist.Where(p => p.PlaylistID == _playlist.PlaylistID).FirstOrDefault()?.LastEditDate < _playlist.LastEditDate)
                    {
                        _context.Playlist.Where(
                            p => p.PlaylistName == _playlist.PlaylistName
                        ).FirstOrDefault().LastEditDate = System.DateTime.Now; //use .Now to push the udpate date past the modified date. Arguably this date should be the date we modified the database playlist, not the file itself, though it is related
                        _context.SaveChanges();
                        LoggingUtils.GenerationLogWriteData($"Playlist '{_playlist.PlaylistName}' already exists, but was modified. Merging...");
                        return SyncOperation.Update;
                    } else
                    {
                        LoggingUtils.GenerationLogWriteData($"Playlist '{_playlist.PlaylistName}' already exists, but was not modified. Skipping all updates.");
                        return SyncOperation.Skip;
                    }
                }
                else
                {
                    LoggingUtils.GenerationLogWriteData($"Overwriting playlist '{_playlist.PlaylistName}'");
                    _context.PlaylistTracks.RemoveRange(_context.PlaylistTracks.Where(plt => plt.PlaylistID == _playlist.PlaylistID));
                    _context.SaveChanges();

                    _context.Playlist.Add(_playlist);
                    _context.SaveChanges();

                    return SyncOperation.Insert;
                }
            }
            _context.Playlist.Add(_playlist);
            _context.SaveChanges();

            _playlist.PlaylistID = _context.Playlist.Where(
                p => p.PlaylistName == _playlist.PlaylistName &&
                p.PlaylistDescription == _playlist.PlaylistDescription
            ).FirstOrDefault().PlaylistID;

            return SyncOperation.Insert;
        }

        public int GetPlaylistID()
        {
            return _playlist.PlaylistID;
        }
    }
}
