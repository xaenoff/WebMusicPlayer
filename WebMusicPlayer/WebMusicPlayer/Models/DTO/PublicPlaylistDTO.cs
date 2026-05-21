namespace WebMusicPlayer.Models.DTO
{
    public class PublicPlaylistDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? OwnerEmail { get; set; }      // Email владельца (для публичных) или "System" для системных
        public bool IsSystem { get; set; }            // Флаг: системный плейлист (создан админом)
        public DateTime CreatedAt { get; set; }
        public string? CoverPath { get; set; }        // Путь к обложке
        public int TrackCount { get; set; }           // Количество треков в плейлисте
        public string? GenreFilter { get; set; }      // Опционально: основной жанр для фильтрации
        public string? FirstTrackCoverPath { get; set; }
    }
}