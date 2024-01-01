using MusicDatabaseGenerator;
using MusicDatabaseGenerator.Synchronizers;

namespace PlaylistTransferTool.Synchronizers
{
    public class PlaylistTrackSynchronizer
    {
        private PlaylistTracks[] _playlistTracks;
        private MusicLibraryContext _context;

        public PlaylistTrackSynchronizer(PlaylistTracks[] playlistTracks, MusicLibraryContext context)
        {
            _playlistTracks = playlistTracks;
            _context = context;
        }

        public SyncOperation Insert()
        {
            foreach(var plt in _playlistTracks)
            {
                _context.PlaylistTracks.Add(plt);
            }

            _context.SaveChanges();

            return SyncOperation.Insert;
        }
    }
}
