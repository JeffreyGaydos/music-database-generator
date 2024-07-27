namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PlaylistTracks
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PlaylistID { get; set; }

        public int? TrackID { get; set; }

        public int? TrackOrder { get; set; }

        [StringLength(260)]
        public string LastKnownPath { get; set; }
    }
}
