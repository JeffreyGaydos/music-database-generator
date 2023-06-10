namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class LinkedTrack
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TrackID1 { get; set; }

        public int? TrackID2 { get; set; }
    }
}
