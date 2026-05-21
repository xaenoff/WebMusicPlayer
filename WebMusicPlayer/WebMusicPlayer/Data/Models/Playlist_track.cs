namespace WebMusicPlayer.Data.Models
{
    public class Playlist_track
    {
        public int PlaylistId { get; set; }
        public int TrackId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;

        public Playlist? Playlist { get; set; }
        public Track? Track { get; set; }
    }
}