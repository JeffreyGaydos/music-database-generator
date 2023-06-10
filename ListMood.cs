namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ListMood")]
    public partial class ListMood
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long MoodID { get; set; }

        [StringLength(100)]
        public string MoodDesc { get; set; }
    }
}
