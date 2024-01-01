using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TagLib.Matroska;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class MainSynchonizer : ASynchronizer, ISynchronizer
    {
        private static List<int> _idsSeen = new List<int>();
        private const int SQLDateTimeMaxPrecision = 99999;
        
        public static List<int> TrackIDsDeleted = new List<int>();

        public MainSynchonizer(MusicLibraryTrack mlt, MusicLibraryContext context, LoggingUtils logger) {
            _mlt = mlt;
            _context = context;
            _logger = logger;

            if(_idsSeen == null)
            {
                _idsSeen = new List<int>();
            }
        }

        public SyncOperation Synchronize()
        {
            if(_context.Main.Any(m => m.ISRC == _mlt.main.ISRC && m.Duration == _mlt.main.Duration && m.Title == _mlt.main.Title))
            {
                return Update();
            } else
            {
                return Insert();
            }
        }

        internal override SyncOperation Insert()
        {
            _mlt.main.GeneratedDate = DateTime.Now;
            _context.Main.Add(_mlt.main);
            _context.SaveChanges();
            int? trackID = _context.Main.ToList().Where(m => m == _mlt.main).FirstOrDefault()?.TrackID;
            if (trackID == null)
            {
                throw new Exception($"Could not create a Main record for track {_mlt.main.Title}");
            }
            _mlt.main.TrackID = trackID.Value;
            _idsSeen.Add(trackID.Value);
            return SyncOperation.Insert;
        }

        internal override SyncOperation Update()
        {
            Main match = _context.Main.First(m => m.ISRC == _mlt.main.ISRC && m.Duration == _mlt.main.Duration && m.Title == _mlt.main.Title);
            _mlt.main.TrackID = match.TrackID; //maintain ID so other mappings remain sound
            _idsSeen.Add(match.TrackID);
            if(match.Title == "Fade Out")
            {
                Console.WriteLine("Test case");
            }
            if (SQLCSharpDateTimeComparison(match.LastModifiedDate, _mlt.main.LastModifiedDate) <= 0)
            {
                match.GeneratedDate = _mlt.main.GeneratedDate;
                return SyncOperation.Skip;
            }
            string differences = DataEquivalent(_mlt.main, match);            
            if (differences == ""
                || (NumberOfNulls(match) > NumberOfNulls(_mlt.main))
                || (NumberOfNulls(match) == NumberOfNulls(_mlt.main) && match.Bitrate >= _mlt.main.Bitrate)
               )
            {
                return SyncOperation.None; //no-op data-wise
            } else
            {
                match.SampleRate = _mlt.main.SampleRate;
                match.Duration = _mlt.main.Duration;
                match.Title = _mlt.main.Title;
                match.Channels = _mlt.main.Channels;
                match.Publisher = _mlt.main.Publisher;
                match.Volume = _mlt.main.Volume;
                match.BitsPerSample = _mlt.main.BitsPerSample;
                match.BeatsPerMin = _mlt.main.BeatsPerMin;
                match.Comment = _mlt.main.Comment;
                match.Copyright = _mlt.main.Copyright;
                match.FilePath = _mlt.main.FilePath;
                match.ISRC = _mlt.main.ISRC;
                match.LinkedTrackPlaylistID = _mlt.main.LinkedTrackPlaylistID;
                match.Owner = _mlt.main.Owner;
                match.ReleaseYear = _mlt.main.ReleaseYear;
                match.Bitrate = _mlt.main.Bitrate;
                match.Lyrics = _mlt.main.Lyrics;
                match.Rating = _mlt.main.Rating;

                match.AddDate = _mlt.main.AddDate;
                _mlt.main.GeneratedDate = DateTime.Now;
                match.GeneratedDate = _mlt.main.GeneratedDate;
                match.LastModifiedDate = _mlt.main.LastModifiedDate;
                _context.SaveChanges();

                _logger.GenerationLogWriteData($"Updated fields {differences}", true);
                return SyncOperation.Update;
            }
        }

        public static new SyncOperation Delete()
        {
            if (_context.Main.Where(m => !_idsSeen.Contains(m.TrackID)).Any())
            {
                TrackIDsDeleted = _context.Main.Where(m => !_idsSeen.Contains(m.TrackID)).Select(t => t.TrackID).ToList();
                _logger.GenerationLogWriteData($"Deleted {_context.Main.Where(m => !_idsSeen.Contains(m.TrackID)).Count()} entry from Main");
                _context.Main.RemoveRange(_context.Main.Where(m => !_idsSeen.Contains(m.TrackID)));
                _context.SaveChanges();
                return SyncOperation.Delete;
            }
            return SyncOperation.None;
        }

        private int SQLCSharpDateTimeComparison(DateTime? date1, DateTime? date2)
        {
            long dateDiff = (date2 ?? DateTime.MinValue).Ticks - (date1 ?? DateTime.MinValue).Ticks;
            if(dateDiff > 0 && dateDiff - SQLDateTimeMaxPrecision > 0)
            {
                return 1;
            } else if (dateDiff < 0 && dateDiff + SQLDateTimeMaxPrecision < 0)
            {
                return -1;
            } else {
                return 0;
            }
        }

        private string DataEquivalent(Main self, Main other)
        {
            List<string> diffs = new List<string>();
            if(self == other) return string.Join(",", diffs);
            if(self == null || other == null)
            {
                diffs.Add("object was null");
                return string.Join(",", diffs);
            }
            if(self.SampleRate != other.SampleRate)
            {
                diffs.Add(nameof(self.SampleRate));
                LogDiff(nameof(self.SampleRate), self.SampleRate, other.SampleRate);
            }
            if (self.Duration != other.Duration)
            {
                diffs.Add(nameof(self.Duration));
                LogDiff(nameof(self.Duration), self.Duration, other.Duration);
            }
            if(self.Title != other.Title)
            {
                diffs.Add(nameof(self.Title));
                LogDiff(nameof(self.Title), self.Title, other.Title);
            }
            if(self.Channels != other.Channels)
            {
                diffs.Add(nameof(self.Channels));
                LogDiff(nameof(self.Channels), self.Channels, other.Channels);
            }
            if(self.Publisher != other.Publisher)
            {
                diffs.Add(nameof(self.Publisher));
                LogDiff(nameof(self.Publisher), self.Publisher, other.Publisher);
            }
            if(self.Volume != other.Volume)
            {
                diffs.Add(nameof(self.Volume));
                LogDiff(nameof(self.Volume), self.Volume, other.Volume);
            }
            if(self.BitsPerSample != other.BitsPerSample)
            {
                diffs.Add(nameof(self.BitsPerSample));
                LogDiff(nameof(self.BitsPerSample), self.BitsPerSample, other.BitsPerSample);
            }
            if(self.BeatsPerMin != other.BeatsPerMin)
            {
                diffs.Add(nameof(self.BeatsPerMin));
                LogDiff(nameof(self.BeatsPerMin), self.BeatsPerMin, other.BeatsPerMin);
            }
            if(self.Comment != other.Comment)
            {
                diffs.Add(nameof(self.Comment));
                LogDiff(nameof(self.Comment), self.Comment, other.Comment);
            }
            if(self.Copyright != other.Copyright)
            {
                diffs.Add(nameof(self.Copyright));
                LogDiff(nameof(self.Copyright), self.Copyright, other.Copyright);
            }
            if(self.FilePath != other.FilePath)
            {
                diffs.Add(nameof(self.FilePath));
                LogDiff(nameof(self.FilePath), self.FilePath, other.FilePath);
            }
            if(self.ISRC != other.ISRC)
            {
                diffs.Add(nameof(self.ISRC));
                LogDiff(nameof(self.ISRC), self.ISRC, other.ISRC);
            }
            if(self.LinkedTrackPlaylistID != other.LinkedTrackPlaylistID)
            {
                diffs.Add(nameof(self.LinkedTrackPlaylistID));
                LogDiff(nameof(self.LinkedTrackPlaylistID), self.LinkedTrackPlaylistID, other.LinkedTrackPlaylistID);
            }
            if(self.Owner != other.Owner)
            {
                diffs.Add(nameof(self.Owner));
                LogDiff(nameof(self.Owner), self.Owner, other.Owner);
            }
            if(self.ReleaseYear != other.ReleaseYear)
            {
                diffs.Add(nameof(self.ReleaseYear));
                LogDiff(nameof(self.ReleaseYear), self.ReleaseYear, other.ReleaseYear);
            }
            if(self.Bitrate != other.Bitrate)
            {
                diffs.Add(nameof(self.Bitrate));
                LogDiff(nameof(self.Bitrate), self.Bitrate, other.Bitrate);
            }
            if(self.Lyrics != other.Lyrics)
            {
                diffs.Add(nameof(self.Lyrics));
                LogDiff(nameof(self.Lyrics), self.Lyrics, other.Lyrics);
            }
            if (self.Rating != other.Rating)
            {
                diffs.Add(nameof(self.Rating));
                LogDiff(nameof(self.Rating), self.Rating, other.Rating);
            }

            return string.Join(",", diffs);
        }

        private void LogDiff<T>(string fieldName, T self, T other)
        {
            _logger.GenerationLogWriteData($"    {fieldName}: {self} - {other}", true);
        }

        private int NumberOfNulls(Main obj)
        {
            int count = 0;
            if(obj.SampleRate == null) count++;
            if(obj.Duration == null) count++;
            if(obj.Title == null) count++;
            if(obj.Channels == null) count++;
            if(obj.Publisher == null) count++;
            if(obj.Volume == null) count++;
            if(obj.BitsPerSample == null) count++;
            if(obj.BeatsPerMin == null) count++;
            if(obj.Comment == null) count++;
            if(obj.Copyright == null) count++;
            if(obj.FilePath == null) count++;
            if(obj.ISRC == null) count++;
            if(obj.LinkedTrackPlaylistID == null) count++;
            if(obj.Owner == null) count++;
            if(obj.ReleaseYear == null) count++;
            if(obj.Bitrate == null) count++;
            if(obj.Lyrics == null) count++;
            if(obj.Rating == null) count++;
            if(obj.AddDate == null) count++;
            if(obj.GeneratedDate == null) count++;
            if(obj.LastModifiedDate == null) count++;
            return count;
        }
    }
}
