namespace MusicDatabaseGenerator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AlbumArt")]
    public partial class AlbumArt
    {
        [Key]
        [StringLength(260)]
        public string AlbumArtPath { get; set; }

        [StringLength(7)]
        public string PrimaryColor { get; set; }

        public int? AlbumID { get; set; }

        public byte[] RawData { get; set; }
    }
}
