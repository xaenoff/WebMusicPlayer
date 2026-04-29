namespace WebMusicPlayer.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public bool IsEmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Playlist>? Playlist { get; set; }
        public ICollection<Favorite>? Favorite { get; set; }
    }
}