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

        public virtual DbSet<Album> Album { get; set; }
        public virtual DbSet<AlbumTracks> AlbumTracks { get; set; }
        public virtual DbSet<Artist> Artist { get; set; }
        public virtual DbSet<ArtistPersons> ArtistPersons { get; set; }
        public virtual DbSet<ArtistTracks> ArtistTracks { get; set; }
        public virtual DbSet<Genre> Genre { get; set; }
        public virtual DbSet<GenreTracks> GenreTracks { get; set; }
        public virtual DbSet<LinkedTracks> LinkedTracks { get; set; }
        public virtual DbSet<Main> Main { get; set; }
        public virtual DbSet<Mood> Mood { get; set; }
        public virtual DbSet<MoodTracks> MoodTracks { get; set; }
        public virtual DbSet<Playlist> Playlist { get; set; }
        public virtual DbSet<PlaylistTracks> PlaylistTracks { get; set; }
        public virtual DbSet<PlayLogs> PlayLogs { get; set; }
        public virtual DbSet<LeadArtists> LeadArtists { get; set; }
        public virtual DbSet<MainDataJoined> MainDataJoined { get; set; }

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

            modelBuilder.Entity<Main>()
                .Property(e => e.Copyright)
                .IsUnicode(false);

            modelBuilder.Entity<Main>()
                .Property(e => e.Publisher)
                .IsUnicode(false);

            modelBuilder.Entity<Main>()
                .Property(e => e.ISRC)
                .IsUnicode(false);

            modelBuilder.Entity<MainDataJoined>()
                .Property(e => e.Duration)
                .HasPrecision(18, 0);

            modelBuilder.Entity<MainDataJoined>()
                .Property(e => e.FilePath)
                .IsUnicode(false);

            modelBuilder.Entity<MainDataJoined>()
                .Property(e => e.AverageDecibels)
                .HasPrecision(18, 0);

            modelBuilder.Entity<MainDataJoined>()
                .Property(e => e.Copyright)
                .IsUnicode(false);

            modelBuilder.Entity<MainDataJoined>()
                .Property(e => e.Publisher)
                .IsUnicode(false);

            modelBuilder.Entity<MainDataJoined>()
                .Property(e => e.ISRC)
                .IsUnicode(false);
        }
    }
}
