namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Album")]
    public partial class Album
    {
        public int AlbumID { get; set; }

        [StringLength(1000)]
        public string AlbumName { get; set; }

        public int? TrackCount { get; set; }

        public int? ReleaseYear { get; set; }
    }
}
