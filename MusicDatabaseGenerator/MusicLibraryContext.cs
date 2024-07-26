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
        public virtual DbSet<AlbumArt> AlbumArt { get; set; }
        public virtual DbSet<AlbumTracks> AlbumTracks { get; set; }
        public virtual DbSet<Artist> Artist { get; set; }
        public virtual DbSet<ArtistPersons> ArtistPersons { get; set; }
        public virtual DbSet<ArtistTracks> ArtistTracks { get; set; }
        public virtual DbSet<Genre> Genre { get; set; }
        public virtual DbSet<GenreTracks> GenreTracks { get; set; }
        public virtual DbSet<Main> Main { get; set; }
        public virtual DbSet<Playlist> Playlist { get; set; }
        public virtual DbSet<PlaylistTracks> PlaylistTracks { get; set; }
        public virtual DbSet<PlayLogs> PlayLogs { get; set; }
        public virtual DbSet<TrackPersons> TrackPersons { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AlbumArt>()
                .Property(e => e.AlbumArtPath)
                .IsUnicode(false);

            modelBuilder.Entity<AlbumArt>()
                .Property(e => e.PrimaryColor)
                .IsUnicode(false);

            modelBuilder.Entity<Main>()
                .Property(e => e.Duration)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Main>()
                .Property(e => e.FilePath)
                .IsUnicode(false);

            modelBuilder.Entity<Main>()
                .Property(e => e.ISRC)
                .IsUnicode(false);

            modelBuilder.Entity<PlaylistTracks>()
                .Property(e => e.LastKnownPath)
                .IsUnicode(false);
        }
    }
}
