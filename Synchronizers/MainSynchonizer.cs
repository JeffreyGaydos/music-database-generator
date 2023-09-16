using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicDatabaseGenerator.Synchronizers
{
    public class MainSynchonizer : ASynchronizer, ISynchronizer
    {
        public MainSynchonizer(MusicLibraryTrack mlt, MusicLibraryContext context) {
            _mlt = mlt;
            _context = context;
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
            return SyncOperation.Insert;
        }

        internal override SyncOperation Update()
        {
            Main match = _context.Main.First(m => m.ISRC == _mlt.main.ISRC && m.Duration == _mlt.main.Duration && m.Title == _mlt.main.Title);
            _mlt.main.TrackID = match.TrackID; //maintain ID so other mappings remain sound
            string differences = DataEquivalent(_mlt.main, match);            
            if (differences == "")
            {
                return SyncOperation.None; //no-op data-wise
            } else
            {
                match.SampleRate = _mlt.main.SampleRate;
                match.Duration = _mlt.main.Duration;
                match.Title = _mlt.main.Title;
                match.Channels = _mlt.main.Channels;
                match.Publisher = _mlt.main.Publisher;
                match.AverageDecibels = _mlt.main.AverageDecibels;
                match.BitsPerSample = _mlt.main.BitsPerSample;
                match.BeatsPerMin = _mlt.main.BeatsPerMin;
                match.Comment = _mlt.main.Comment;
                match.Copyright = _mlt.main.Copyright;
                match.FilePath = _mlt.main.FilePath;
                match.ISRC = _mlt.main.ISRC;
                match.Linked = _mlt.main.Linked;
                match.Owner = _mlt.main.Owner;
                match.ReleaseYear = _mlt.main.ReleaseYear;
                match.Bitrate = _mlt.main.Bitrate;
                match.Lyrics = _mlt.main.Lyrics;

                match.AddDate = _mlt.main.AddDate;
                _mlt.main.GeneratedDate = DateTime.Now;
                match.GeneratedDate = _mlt.main.GeneratedDate;
                match.LastModifiedDate = _mlt.main.LastModifiedDate;
                _context.SaveChanges();

                return SyncOperation.Update;
            }
        }

        internal override void Delete()
        {
            base.Delete();
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
                
            }
            if (self.Duration != other.Duration)
            {
                diffs.Add(nameof(self.Duration));
                
            }
            if(self.Title != other.Title)
            {
                diffs.Add(nameof(self.Title));
                
            }
            if(self.Channels != other.Channels)
            {
                diffs.Add(nameof(self.Channels));
                
            }
            if(self.Publisher != other.Publisher)
            {
                diffs.Add(nameof(self.Publisher));
                
            }
            if(self.AverageDecibels != other.AverageDecibels)
            {
                diffs.Add(nameof(self.AverageDecibels));
                
            }
            if(self.BitsPerSample != other.BitsPerSample)
            {
                diffs.Add(nameof(self.BitsPerSample));
                
            }
            if(self.BeatsPerMin != other.BeatsPerMin)
            {
                diffs.Add(nameof(self.BeatsPerMin));
                
            }
            if(self.Comment != other.Comment)
            {
                diffs.Add(nameof(self.Comment));
                
            }
            if(self.Copyright != other.Copyright)
            {
                diffs.Add(nameof(self.Copyright));
                
            }
            if(self.FilePath != other.FilePath)
            {
                diffs.Add(nameof(self.FilePath));
                
            }
            if(self.ISRC != other.ISRC)
            {
                diffs.Add(nameof(self.ISRC));
                
            }
            if(self.Linked != other.Linked)
            {
                diffs.Add(nameof(self.Linked));
                
            }
            if(self.Owner != other.Owner)
            {
                diffs.Add(nameof(self.Owner));
                
            }
            if(self.ReleaseYear != other.ReleaseYear)
            {
                diffs.Add(nameof(self.ReleaseYear));
                
            }
            if(self.Bitrate != other.Bitrate)
            {
                diffs.Add(nameof(self.Bitrate));
                
            }
            if(self.Lyrics != other.Lyrics)
            {
                diffs.Add(nameof(self.Lyrics));
                
            }

            return string.Join(",", diffs);
        }
    }
}
