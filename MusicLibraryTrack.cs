﻿using System;
using System.Collections.Generic;
using System.Linq;

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
        public List<Album> album = new List<Album>();

        //Derived Fields
        //public List<GenreTracks> genreTracks = new List<GenreTracks>();
        //public List<ArtistTracks> artistTracks = new List<ArtistTracks>();
        public List<AlbumTracks> albumTracks = new List<AlbumTracks>(); // partially derived field

        //Non-Generated Fields
        //public LinkedTracks linkedTracks;
        //public Owner owner;
        //public List<Playlist> playlists = new List<Playlist>();
        //public List<PlaylistTracks> playlistTracks = new List<PlaylistTracks>();
        //public List<PlayLogs> playLogs = new List<PlayLogs>();

        private MusicLibraryContext _context;

        public MusicLibraryTrack(MusicLibraryContext context) {
            _context = context;
        }

        public void Sync()
        {
            List<string> currentGenres = _context.Genre.Select(g => g.GenreName).ToList();
            List<string> currentArtists = _context.Artist.Select(a => a.ArtistName).ToList();
            List<string> currentAlbums = _context.Album.Select(a => a.AlbumName).ToList();

            _context.Main.Add(main);
            _context.SaveChanges();
            
            int? trackID = _context.Main.ToList().Where(m => m == main).FirstOrDefault()?.TrackID;
            if (trackID == null)
            {
                throw new Exception($"Could not create a Main record for track {main.Title}");
            }

            foreach (string newGenre in genre.Select(g => g.GenreName).ToList().Except(currentGenres))
            {
                Genre fullGenre = genre.Where(g => g.GenreName == newGenre).FirstOrDefault();
                if (fullGenre != null)
                {
                    _context.Genre.Add(fullGenre);
                    _context.SaveChanges();
                }
            }

            foreach(Genre g in genre)
            {
                g.GenreID = _context.Genre.ToList()
                        .Where(dbg => dbg.GenreName == g.GenreName)
                        .FirstOrDefault().GenreID;

                GenreTracks genreTrack = new GenreTracks();
                genreTrack.TrackID = trackID.Value;
                genreTrack.GenreID = g.GenreID;
                _context.GenreTracks.Add(genreTrack);
            }

            foreach (string newArtist in artist.Select(a => a.ArtistName).ToList().Except(currentArtists))
            {
                Artist fullArtist = artist.Where(a => a.ArtistName == newArtist).FirstOrDefault();
                if(fullArtist != null)
                {
                    _context.Artist.Add(fullArtist);
                    _context.SaveChanges();

                    fullArtist.ArtistID = _context.Artist.ToList()
                        .Where(a => a == fullArtist)
                        .FirstOrDefault().ArtistID;
                }
            }

            foreach(Artist a in artist)
            {
                a.ArtistID = _context.Artist.ToList()
                        .Where(dba => dba.ArtistName == a.ArtistName)
                        .FirstOrDefault().ArtistID;

                ArtistTracks artistTrack = new ArtistTracks();
                artistTrack.TrackID = trackID.Value;
                artistTrack.ArtistID = a.ArtistID;
                _context.ArtistTracks.Add(artistTrack);
            }

            foreach(string newAlbum in album.Select(a => a.AlbumName).ToList().Except(currentAlbums))
            {
                Album fullAlbum = album.Where(a => a.AlbumName == newAlbum).FirstOrDefault();
                if(fullAlbum != null)
                {
                    _context.Album.Add(fullAlbum);
                    _context.SaveChanges();

                    fullAlbum.AlbumID = _context.Album.ToList()
                        .Where(a => a == fullAlbum)
                        .FirstOrDefault().AlbumID;
                }
            }

            foreach(Album a in album)
            {
                a.AlbumID = _context.Album.ToList()
                        .Where(dba => dba.AlbumName == a.AlbumName)
                        .FirstOrDefault().AlbumID;

                AlbumTracks albumTrack = new AlbumTracks();
                albumTrack.TrackID = trackID.Value;
                albumTrack.AlbumID = a.AlbumID;
                albumTrack.TrackOrder = albumTracks
                    .Where(ats => ats.AlbumID.Equals(a.AlbumID))
                    .Select(ats => ats.TrackOrder)
                    .FirstOrDefault();
                _context.AlbumTracks.Add(albumTrack);
            }

            _context.SaveChanges();
            
            Console.WriteLine($"Finished processing track {trackIndex} ({main.Title})");
        }
    }
}
