using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace MusicDatabaseGenerator
{
    public partial class MusicLibraryContext : DbContext
    {
        public MusicLibraryContext()
            : base("name=MusicLibraryContext")
        {
        }

        public virtual DbSet<Album> Albums { get; set; }
        public virtual DbSet<LinkedTrack> LinkedTracks { get; set; }
        public virtual DbSet<ListArtist> ListArtists { get; set; }
        public virtual DbSet<ListGenre> ListGenres { get; set; }
        public virtual DbSet<ListMood> ListMoods { get; set; }
        public virtual DbSet<ListOwner> ListOwners { get; set; }
        public virtual DbSet<Main> Mains { get; set; }
        public virtual DbSet<Playlist> Playlists { get; set; }
        public virtual DbSet<PlaylistTrack> PlaylistTracks { get; set; }
        public virtual DbSet<PlayLog> PlayLogs { get; set; }

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
