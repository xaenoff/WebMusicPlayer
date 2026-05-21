namespace WebMusicPlayer.Data.Models
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? OwnerId { get; set; } // NULL = системный плейлист
        public bool IsPublic { get; set; }
        public bool IsSystem { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User? Owner { get; set; }
        public ICollection<Playlist_track>? Playlist_track { get; set; }
        public string? CoverPath { get; set; }
    }
}