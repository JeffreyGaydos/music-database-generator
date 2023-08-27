using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TagLib.Mpeg;

namespace MusicDatabaseGenerator
{
    internal class SQLStringComparison : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == y) return true;
            if (x?.ToLower() == y?.ToLower()) return true;
            return string.Compare(x?.ToLower(), y?.ToLower(), CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0;
        }

        public int GetHashCode(string obj)
        {
            return obj?.ToLower().GetHashCode() ?? 0;
        }
    }

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
        public List<AlbumArt> albumArt = new List<AlbumArt>();
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

        private MusicLibraryContext _context;
        private LoggingUtils _logger;
        private int _trackID;

        private static Regex R_PKViolationTable = new Regex("dbo\\.[a-zA-Z]+", RegexOptions.Compiled);
        private static Regex R_PKViolationViolatingKey = new Regex("(?<=\\().+(?=\\))", RegexOptions.Compiled);

        private int _totalCount;

        public MusicLibraryTrack(MusicLibraryContext context, LoggingUtils logger, int count) {
            _context = context;
            _logger = logger;
            _totalCount = count;
        }

        public void Sync()
        {
            if (albumArt.Any())
            {
                AddAlbumArt();

                _logger.GenerationLogWriteData($"{100 * albumArtIndex / (decimal)_totalCount:00.00}% Finished processing album art {albumArtIndex} ({albumArt.FirstOrDefault().AlbumArtPath})");
            } else
            {
                using (DbContextTransaction transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        AddMainData();
                        AddGenreData();
                        AddArtistData();
                        AddArtistPersonsData(); //must come after the artist data
                        AddTrackPersonsData();
                        AddAlbumData();

                        PostProcessing();

                        transaction.Commit();

                        _logger.GenerationLogWriteData($"{100 * trackIndex / (decimal)_totalCount:00.00}% Finished processing track {trackIndex} ({main.Title})");
                    }
                    catch (DbUpdateException ue)
                    {
                        string innerMessage = ue.InnerException.InnerException.Message;
                        if (innerMessage.Contains("Violation of UNIQUE KEY constraint"))
                        {
                            string key = R_PKViolationViolatingKey.Match(innerMessage).Value;
                            string table = R_PKViolationTable.Match(innerMessage).Value;
                            _logger.GenerationLogWriteData($"{100 * trackIndex / (decimal)_totalCount:00.00}% Finished processing track {trackIndex} DUPLICATE (skipped) ({main.Title})");
                            _logger.DuplicateLogWriteData($"{main.FilePath}: UNIQUE constraint violation on table '{table}' from key: [{key}]");
                            transaction.Rollback();
                            //The first entry in the change tracker is the most recent change, i.e. the thing that threw an exception. We need to remove it from the context
                            _context.ChangeTracker.Entries().First().State = EntityState.Detached;
                        }
                    }
                }
            }
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

            foreach (string newGenre in genre.Select(g => g.GenreName).Where(gn => !currentGenres.Contains(gn, new SQLStringComparison())).ToList())
            {
                List<string> newGenreList = new List<string> { newGenre };
                Genre fullGenre = genre.Where(g => newGenreList.Contains(g.GenreName, new SQLStringComparison())).FirstOrDefault();
                if (fullGenre != null)
                {
                    _context.Genre.Add(fullGenre);
                    _context.SaveChanges();
                }
            }

            foreach (Genre g in genre)
            {
                g.GenreID = _context.Genre.ToList()
                        .Where(dbg => {
                            List<string> list = new List<string> { dbg.GenreName };
                            return list.Contains(g.GenreName, new SQLStringComparison());
                        })
                        .FirstOrDefault().GenreID;

                GenreTracks genreTrack = new GenreTracks();
                genreTrack.TrackID = _trackID;
                genreTrack.GenreID = g.GenreID;
                _context.GenreTracks.Add(genreTrack);
            }
            _context.SaveChanges();
        }

        private void AddArtistData()
        {
            List<string> currentArtists = _context.Artist.Select(a => a.ArtistName).ToList();

            foreach (string newArtist in artist.Select(a => a.ArtistName).Where(a => !currentArtists.Contains(a, new SQLStringComparison())).ToList())
            {
                List<string> newArtistList = new List<string> { newArtist };
                Artist fullArtist = artist.Where(a => newArtistList.Contains(a.ArtistName, new SQLStringComparison())).FirstOrDefault();
                if (fullArtist != null)
                {
                    _context.Artist.Add(fullArtist);
                    _context.SaveChanges();
                }
            }

            foreach (Artist a in artist)
            {
                a.ArtistID = _context.Artist.ToList()
                        .Where(dba => {
                            List<string> list = new List<string> { dba.ArtistName };
                            return list.Contains(a.ArtistName, new SQLStringComparison());
                        })
                        .FirstOrDefault().ArtistID;

                ArtistTracks artistTrack = new ArtistTracks();
                artistTrack.TrackID = _trackID;
                artistTrack.ArtistID = a.ArtistID;
                _context.ArtistTracks.Add(artistTrack);
            }
            _context.SaveChanges();
        }

        private void AddAlbumData()
        {
            List<string> currentAlbums = _context.Album.Select(a => a.AlbumName).ToList();

            foreach (string newAlbum in album.Select(a => a.Item1.AlbumName).Where(a => !currentAlbums.Contains(a, new SQLStringComparison())).ToList())
            {
                List<string> newAlbumList = new List<string> { newAlbum };
                Album fullAlbum = album.Where(a => newAlbumList.Contains(a.Item1.AlbumName, new SQLStringComparison())).FirstOrDefault().Item1;
                if (fullAlbum != null)
                {
                    _context.Album.Add(fullAlbum);
                    _context.SaveChanges();
                }
            }

            foreach ((Album, AlbumTracks) a in album)
            {
                a.Item1.AlbumID = _context.Album.ToList()
                        .Where(dba => {
                            List<string> list = new List<string> { dba.AlbumName };
                            return list.Contains(a.Item1.AlbumName, new SQLStringComparison());
                        })
                        .FirstOrDefault().AlbumID;

                a.Item2.AlbumID = a.Item1.AlbumID;
                a.Item2.TrackID = _trackID;
                _context.AlbumTracks.Add(a.Item2);
            }
            _context.SaveChanges();
        }

        private void AddArtistPersonsData()
        {
            foreach(ArtistPersons person in artistPersons)
            {
                //If there are > 1 artists on 1 track, the standard mp3 metadata on performers is not very useful
                //and likely will not contain the information needed to map performers to specific artists. In this
                //case, the artistID that we map here may be inaccurate or relate to a different artist on the same
                //track
                person.ArtistID = artist.Select(a => a.ArtistID).FirstOrDefault();
                if (!_context.ArtistPersons.Select(p => p.PersonName).Contains(person.PersonName))
                {
                    _context.ArtistPersons.Add(person);
                }
            }
            _context.SaveChanges();
        }

        private void AddTrackPersonsData()
        {
            foreach((TrackPersons tp, string name) person in trackPersons)
            {
                person.tp.PersonID = _context.ArtistPersons.Where(ap => ap.PersonName == person.name).Select(a => a.PersonID).FirstOrDefault();
                person.tp.TrackID = main.TrackID;
                if(person.tp.PersonID != 0)
                    _context.TrackPersons.Add(person.tp);
            }
            _context.SaveChanges();
        }

        private Regex parentDirectoryMatch = new Regex(".*(?=[\\\\\\/][^\\\\\\/]*\\.[a-zA-Z]+)", RegexOptions.Compiled);
        private void AddAlbumArt()
        {
            List<AlbumTracks> existingAlbumTracks = _context.AlbumTracks.ToList();
            foreach(AlbumArt art in albumArt)
            {
                string parentDirectory = parentDirectoryMatch.Match(art.AlbumArtPath).Value;
                int trackID = _context.Main
                    .Where(m => m.FilePath.StartsWith(parentDirectory))
                    .Select(t => t.TrackID).FirstOrDefault();

                art.AlbumID = existingAlbumTracks.Where(at => at.TrackID == trackID).FirstOrDefault()?.AlbumID;
                _context.AlbumArt.Add(art);
            }
            _context.SaveChanges();
        }

        private void PostProcessing()
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
