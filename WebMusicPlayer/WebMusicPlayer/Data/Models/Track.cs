namespace WebMusicPlayer.Data.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? CoverPath { get; set; }
        public string? Lyrics { get; set; }
        public int? GenreId { get; set; }
        public bool IsAvailable { get; set; } = true;

        public Genre? Genre { get; set; }
        public ICollection<Playlist_track>? Playlist_track { get; set; }
        public ICollection<Favorite>? Favorite { get; set; }
    }
}