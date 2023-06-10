namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ListGenre")]
    public partial class ListGenre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GenreID { get; set; }

        public int? GenreName { get; set; }
    }
}
