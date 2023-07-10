namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ArtistPersons
    {
        [Key]
        public int PersonID { get; set; }

        public int? ArtistID { get; set; }

        [StringLength(1000)]
        public string PersonName { get; set; }
    }
}
