namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Artist")]
    public partial class Artist
    {
        public int ArtistID { get; set; }

        public int? PrimaryPersonID { get; set; }

        [StringLength(100)]
        public string ArtistName { get; set; }
    }
}
