namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Playlist")]
    public partial class Playlist
    {
        public int PlaylistID { get; set; }

        [StringLength(1000)]
        public string PlaylistName { get; set; }

        [StringLength(4000)]
        public string PlaylistDescription { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? LastEditDate { get; set; }
    }
}
