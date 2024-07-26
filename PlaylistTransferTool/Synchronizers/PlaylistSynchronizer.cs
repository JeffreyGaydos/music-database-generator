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
                if (_config.mergePlaylistsWithSameName)
                {
                    LoggingUtils.GenerationLogWriteData($"Playlist '{_playlist.PlaylistName}' already exists.");
                    _playlist.PlaylistID = _context.Playlist.Where(
                        p => p.PlaylistName == _playlist.PlaylistName
                    ).FirstOrDefault().PlaylistID;

                    return SyncOperation.Skip;
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
