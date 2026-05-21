namespace WebMusicPlayer.Data.Models
{
    public class SavedPlaylist
    {
        public int UserId { get; set; }
        public int PlaylistId { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Playlist? Playlist { get; set; }
    }
}