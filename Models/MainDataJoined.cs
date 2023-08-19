namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MainDataJoined")]
    public partial class MainDataJoined
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TrackID { get; set; }

        [StringLength(435)]
        public string Title { get; set; }

        public decimal? Duration { get; set; }

        [StringLength(260)]
        public string FilePath { get; set; }

        public decimal? AverageDecibels { get; set; }

        [StringLength(1000)]
        public string Owner { get; set; }

        public bool? Linked { get; set; }

        public int? ReleaseYear { get; set; }

        public DateTime? AddDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        [StringLength(4000)]
        public string Lyrics { get; set; }

        [StringLength(4000)]
        public string Comment { get; set; }

        public int? BeatsPerMin { get; set; }

        [StringLength(1000)]
        public string Copyright { get; set; }

        [StringLength(1000)]
        public string Publisher { get; set; }

        [StringLength(12)]
        public string ISRC { get; set; }

        public int? Bitrate { get; set; }

        public int? Channels { get; set; }

        public int? SampleRate { get; set; }

        public int? BitsPerSample { get; set; }

        public DateTime? GeneratedDate { get; set; }

        public int? TrackOrder { get; set; }

        [StringLength(446)]
        public string AlbumName { get; set; }

        public int? AlbumReleaseYear { get; set; }

        [StringLength(100)]
        public string GenreName { get; set; }

        [StringLength(100)]
        public string ArtistName { get; set; }
    }
}
