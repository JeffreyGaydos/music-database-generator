using System;
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
            if (DataEquivalent(_mlt.main, match))
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

        private bool DataEquivalent(Main self, Main other)
        {
            if(self == other) return true;
            if(self == null || other == null) return false;
            return self.SampleRate == other.SampleRate
                && self.Duration == other.Duration
                && self.Title == other.Title
                && self.Channels == other.Channels
                && self.Publisher == other.Publisher
                && self.AverageDecibels == other.AverageDecibels
                && self.BitsPerSample == other.BitsPerSample
                && self.BeatsPerMin == other.BeatsPerMin
                && self.Comment == other.Comment
                && self.Copyright == other.Copyright
                && self.FilePath == other.FilePath
                && self.ISRC == other.ISRC
                && self.Linked == other.Linked
                && self.Owner == other.Owner
                && self.ReleaseYear == other.ReleaseYear
                && self.Bitrate == other.Bitrate
                && self.Lyrics == other.Lyrics;
        }
    }
}
