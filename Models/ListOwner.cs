namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ListOwner")]
    public partial class ListOwner
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OwnerID { get; set; }

        [StringLength(1000)]
        public string OwnerName { get; set; }
    }
}
