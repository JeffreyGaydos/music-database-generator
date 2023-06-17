namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ListArtist")]
    public partial class ListArtist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ArtistID { get; set; }

        [StringLength(100)]
        public string ArtistName { get; set; }
    }
}
