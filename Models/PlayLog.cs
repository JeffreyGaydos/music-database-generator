namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PlayLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TrackID { get; set; }

        public DateTime? DatePlayed { get; set; }
    }
}
