namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Owner")]
    public partial class Owner
    {
        public int OwnerID { get; set; }

        [StringLength(1000)]
        public string OwnerName { get; set; }
    }
}
