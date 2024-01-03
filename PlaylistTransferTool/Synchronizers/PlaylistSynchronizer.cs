using MusicDatabaseGenerator;
using MusicDatabaseGenerator.Synchronizers;
using System.Linq;

namespace PlaylistTransferTool.Synchronizers
{
    public class PlaylistSynchronizer
    {
        private Playlist _playlist;
        private MusicLibraryContext _context;

        public PlaylistSynchronizer(Playlist playlist, MusicLibraryContext context)
        {
            _playlist = playlist;
            _context = context;
        }

        public SyncOperation Insert()
        {
            if(_context.Playlist.Where(p => p.PlaylistName == _playlist.PlaylistName).Any())
            {
                LoggingUtils.GenerationLogWriteData($"Playlist {_playlist.PlaylistName} already exists.");
                _playlist.PlaylistID = _context.Playlist.Where(
                    p => p.PlaylistName == _playlist.PlaylistName
                ).FirstOrDefault().PlaylistID;
                return SyncOperation.Skip;
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
