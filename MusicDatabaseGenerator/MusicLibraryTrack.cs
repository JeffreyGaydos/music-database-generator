using System.Collections.Generic;

namespace MusicDatabaseGenerator
{
    public class MusicLibraryTrack
    {
        //NOT the track ID or album art ID in the database
        public static int trackIndex = 0;
        public static int albumArtIndex = 0;

        //Generated Fields
        public Main main;
        public List<Genre> genre = new List<Genre>();
        public List<Artist> artist = new List<Artist>();
        public List<(Album, AlbumTracks)> album = new List<(Album, AlbumTracks)>();
        public AlbumArt albumArt = new AlbumArt();
        public List<ArtistPersons> artistPersons = new List<ArtistPersons>();
        public List<(TrackPersons tp, string name)> trackPersons = new List<(TrackPersons tp, string name)>();

        //Derived Fields
        //public List<GenreTracks> genreTracks = new List<GenreTracks>();
        //public List<ArtistTracks> artistTracks = new List<ArtistTracks>();

        //Non-Generated Fields
        //public LinkedTracks linkedTracks;
        //public Owner owner;
        //public List<Playlist> playlists = new List<Playlist>();
        //public List<PlaylistTracks> playlistTracks = new List<PlaylistTracks>();
        //public List<PlayLogs> playLogs = new List<PlayLogs>();
    }
}
