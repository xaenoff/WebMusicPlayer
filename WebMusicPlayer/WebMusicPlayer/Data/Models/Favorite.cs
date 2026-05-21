namespace WebMusicPlayer.Data.Models
{
    public class Favorite
    {
        public int UserId { get; set; }
        public int TrackId { get; set; }

        public User? User { get; set; }
        public Track? Track { get; set; }
    }
}