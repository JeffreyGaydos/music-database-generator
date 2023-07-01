namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Mood")]
    public partial class Mood
    {
        public int MoodID { get; set; }

        [StringLength(100)]
        public string MoodDesc { get; set; }
    }
}
