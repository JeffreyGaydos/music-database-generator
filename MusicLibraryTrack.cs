using System;
using System.Collections.Generic;
using System.Linq;
using TagLib.Mpeg;

namespace MusicDatabaseGenerator
{
    public class MusicLibraryTrack
    {
        //NOT the track ID
        public static int trackIndex = 0;

        //Generated Fields
        public Main main;
        public List<Genre> genre = new List<Genre>();
        public List<Artist> artist = new List<Artist>();
        public List<(Album, AlbumTracks)> album = new List<(Album, AlbumTracks)>();
        public List<ArtistPersons> artistPersons = new List<ArtistPersons>();

        //Derived Fields
        //public List<GenreTracks> genreTracks = new List<GenreTracks>();
        //public List<ArtistTracks> artistTracks = new List<ArtistTracks>();

        //Non-Generated Fields
        //public LinkedTracks linkedTracks;
        //public Owner owner;
        //public List<Playlist> playlists = new List<Playlist>();
        //public List<PlaylistTracks> playlistTracks = new List<PlaylistTracks>();
        //public List<PlayLogs> playLogs = new List<PlayLogs>();

        private MusicLibraryContext _context;
        private int _trackID;

        public MusicLibraryTrack(MusicLibraryContext context) {
            _context = context;
        }

        public void Sync()
        {
            AddMainData();
            AddGenreData();
            AddArtistData();
            AddArtistPersonsData(); //must come after the artist data
            AddAlbumData();

            LinkCircularIDs();

            _context.SaveChanges();
            
            Console.WriteLine($"Finished processing track {trackIndex} ({main.Title})");
        }

        private void AddMainData()
        {
            main.GeneratedDate = DateTime.Now;
            _context.Main.Add(main);
            _context.SaveChanges();

            int? trackID = _context.Main.ToList().Where(m => m == main).FirstOrDefault()?.TrackID;
            if (trackID == null)
            {
                throw new Exception($"Could not create a Main record for track {main.Title}");
            }
            _trackID = trackID.Value;
        }

        private void AddGenreData()
        {
            List<string> currentGenres = _context.Genre.Select(g => g.GenreName).ToList();

            foreach (string newGenre in genre.Select(g => g.GenreName).ToList().Except(currentGenres))
            {
                Genre fullGenre = genre.Where(g => g.GenreName == newGenre).FirstOrDefault();
                if (fullGenre != null)
                {
                    _context.Genre.Add(fullGenre);
                    _context.SaveChanges();
                }
            }

            foreach (Genre g in genre)
            {
                g.GenreID = _context.Genre.ToList()
                        .Where(dbg => dbg.GenreName == g.GenreName)
                        .FirstOrDefault().GenreID;

                GenreTracks genreTrack = new GenreTracks();
                genreTrack.TrackID = _trackID;
                genreTrack.GenreID = g.GenreID;
                _context.GenreTracks.Add(genreTrack);
            }
        }

        private void AddArtistData()
        {
            List<string> currentArtists = _context.Artist.Select(a => a.ArtistName).ToList();

            foreach (string newArtist in artist.Select(a => a.ArtistName).ToList().Except(currentArtists))
            {
                Artist fullArtist = artist.Where(a => a.ArtistName == newArtist).FirstOrDefault();
                if (fullArtist != null)
                {
                    _context.Artist.Add(fullArtist);
                    _context.SaveChanges();
                }
            }

            foreach (Artist a in artist)
            {
                a.ArtistID = _context.Artist.ToList()
                        .Where(dba => dba.ArtistName == a.ArtistName)
                        .FirstOrDefault().ArtistID;

                ArtistTracks artistTrack = new ArtistTracks();
                artistTrack.TrackID = _trackID;
                artistTrack.ArtistID = a.ArtistID;
                _context.ArtistTracks.Add(artistTrack);
            }
        }

        private void AddAlbumData()
        {
            List<string> currentAlbums = _context.Album.Select(a => a.AlbumName).ToList();

            foreach (string newAlbum in album.Select(a => a.Item1.AlbumName).ToList().Except(currentAlbums))
            {
                Album fullAlbum = album.Where(a => a.Item1.AlbumName == newAlbum).FirstOrDefault().Item1;
                if (fullAlbum != null)
                {
                    _context.Album.Add(fullAlbum);
                    _context.SaveChanges();
                }
            }

            foreach ((Album, AlbumTracks) a in album)
            {
                a.Item1.AlbumID = _context.Album.ToList()
                        .Where(dba => dba.AlbumName == a.Item1.AlbumName)
                        .FirstOrDefault().AlbumID;

                a.Item2.AlbumID = a.Item1.AlbumID;
                a.Item2.TrackID = _trackID;
                _context.AlbumTracks.Add(a.Item2);
            }
        }

        private void AddArtistPersonsData()
        {
            foreach(ArtistPersons person in artistPersons)
            {
                //If there are > 1 artists on 1 track, the standard mp3 metadata on performers is not very useful
                //and likely will not contain the information needed to map performers to specific artists. In this
                //case, the artistID that we map here may be inaccurate or relate to a different artist on the same
                //track
                if(!_context.ArtistPersons.Select(p => p.PersonName).Contains(person.PersonName))
                {
                    person.ArtistID = artist.Select(a => a.ArtistID).First();
                    _context.ArtistPersons.Add(person);
                }
            }
        }

        private void LinkCircularIDs()
        {
            foreach(Artist a in _context.Artist)
            {
                a.PrimaryPersonID = _context.ArtistPersons.Where(ap => ap.ArtistID == a.ArtistID).FirstOrDefault()?.PersonID;
            }

            foreach(Album alb in _context.Album)
            {
                int albumYear = 0;
                List<int> albumTracks = _context.AlbumTracks.Where(at => at.AlbumID == alb.AlbumID).Select(a => a.TrackID).ToList();
                foreach (int TrackID in albumTracks)
                {
                    int trackYear = _context.Main.Where(m => m.TrackID == TrackID).FirstOrDefault()?.ReleaseYear ?? 0;
                    albumYear = trackYear > albumYear ? trackYear : albumYear;
                }
                alb.ReleaseYear = albumYear == 0 ? null : (int?)albumYear;
                alb.TrackCount = albumTracks.Count == 0 ? null : (int?)albumTracks.Count;
            }
        }
    }
}
