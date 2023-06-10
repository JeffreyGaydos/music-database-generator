namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ListExtension")]
    public partial class ListExtension
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ExtensionID { get; set; }

        [StringLength(100)]
        public string FileType { get; set; }
    }
}
