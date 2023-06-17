namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Main")]
    public partial class Main
    {
        [Key]
        public int TrackID { get; set; }

        [StringLength(4000)]
        public string Title { get; set; }

        public decimal? Duration { get; set; }

        [StringLength(260)]
        public string FilePath { get; set; }

        public decimal? AverageDecibels { get; set; }

        public long? MoodIDs { get; set; }

        public int? OwnerID { get; set; }

        public int? GenreID { get; set; }

        public bool? Linked { get; set; }

        public int? ReleaseYear { get; set; }

        public DateTime? AddDate { get; set; }
    }
}
