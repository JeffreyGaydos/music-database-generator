using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace MusicDatabaseGenerator
{
    public partial class MusicLibraryDataContext : DbContext
    {
        public MusicLibraryDataContext()
            : base("name=MusicLibraryDataContext")
        {
        }

        public virtual DbSet<Album> Album { get; set; }
        public virtual DbSet<AlbumTracks> AlbumTracks { get; set; }
        public virtual DbSet<LinkedTracks> LinkedTracks { get; set; }
        public virtual DbSet<ListArtist> ListArtist { get; set; }
        public virtual DbSet<ListGenre> ListGenre { get; set; }
        public virtual DbSet<ListMood> ListMood { get; set; }
        public virtual DbSet<ListOwner> ListOwner { get; set; }
        public virtual DbSet<Main> Main { get; set; }
        public virtual DbSet<Playlist> Playlist { get; set; }
        public virtual DbSet<PlaylistTracks> PlaylistTracks { get; set; }
        public virtual DbSet<PlayLogs> PlayLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Main>()
                .Property(e => e.Duration)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Main>()
                .Property(e => e.FilePath)
                .IsUnicode(false);

            modelBuilder.Entity<Main>()
                .Property(e => e.AverageDecibels)
                .HasPrecision(18, 0);
        }
    }
}
