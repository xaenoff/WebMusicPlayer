using Microsoft.EntityFrameworkCore;
using WebMusicPlayer.Data.Models;

namespace WebMusicPlayer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // “‡·ÎËˆ˚
        public DbSet<User> Users { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Playlist_track> Playlist_track { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<SavedPlaylist> SavedPlaylists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Playlist_track>()
                .HasKey(pt => new { pt.PlaylistId, pt.TrackId });

            modelBuilder.Entity<Favorite>()
                .HasKey(f => new { f.UserId, f.TrackId });

            // ==========================================
            //  Õ¿—“–Œ… ¿ —¬ﬂ«≈… œÀ≈…À»—“Œ¬
            // ==========================================

            modelBuilder.Entity<Playlist>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Playlist)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Playlist_track>()
                .HasOne(pt => pt.Playlist)
                .WithMany(p => p.Playlist_track)
                .HasForeignKey(pt => pt.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Playlist_track>()
                .HasOne(pt => pt.Track)
                .WithMany(t => t.Playlist_track)
                .HasForeignKey(pt => pt.TrackId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==========================================
            //  Õ¿—“–Œ… ¿ —¬ﬂ«≈… »«¡–¿ÕÕŒ√Œ
            // ==========================================

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorite)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Track)
                .WithMany(t => t.Favorite)
                .HasForeignKey(f => f.TrackId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==========================================
            //  Õ¿—“–Œ… ¿ —¬ﬂ«≈… “–≈ Œ¬ » ∆¿Õ–Œ¬
            // ==========================================

            modelBuilder.Entity<Track>()
                .HasOne(t => t.Genre)
                .WithMany(g => g.Track)
                .HasForeignKey(t => t.GenreId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SavedPlaylist>()
                .HasKey(sp => new { sp.UserId, sp.PlaylistId });

            modelBuilder.Entity<SavedPlaylist>()
                .HasOne(sp => sp.User)
                .WithMany(u => u.SavedPlaylists)
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SavedPlaylist>()
                .HasOne(sp => sp.Playlist)
                .WithMany()
                .HasForeignKey(sp => sp.PlaylistId)
                .OnDelete(DeleteBehavior.Restrict); 

        }
    }
}