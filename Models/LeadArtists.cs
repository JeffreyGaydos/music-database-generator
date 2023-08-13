namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class LeadArtists
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ArtistID { get; set; }

        public int? PrimaryPersonID { get; set; }

        [StringLength(100)]
        public string ArtistName { get; set; }

        [StringLength(100)]
        public string PersonName { get; set; }

        [StringLength(100)]
        public string PrimaryPerson { get; set; }
    }
}
