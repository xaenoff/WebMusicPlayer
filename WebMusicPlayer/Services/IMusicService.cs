using System.Threading.Tasks;
using WebMusicPlayer.Data.Models;

namespace WebMusicPlayer.Services
{
    public interface IMusicService
    {
        Task<List<Track>> GetAllTracksAsync();
        Task<Track?> GetTrackByIdAsync(int id);
        Task<List<Track>> GetFavoriteTracksAsync(int userId);
        Task<List<Playlist>> GetAllPlaylistsAsync();
        Task<List<Track>> SearchTracksAsync(string query);
        Task<bool> AddToFavoriteAsync(int userId, int trackId);
        Task<bool> RemoveFromFavoriteAsync(int userId, int trackId);
        Task<bool> IsFavoriteAsync(int userId, int trackId);
        Task<List<Playlist>> GetUserPlaylistsAsync(int userId);
        Task<List<Track>> GetPlaylistTracksAsync(int playlistId);
        Task<int> CreatePlaylistAsync(int userId, string name, List<int> trackIds, string? coverPath = null, bool isPublic = false);
        Task<Playlist?> GetPlaylistByIdAsync(int playlistId);
        Task<Track> CreateTrackAsync(Track track);
        Task<List<Genre>> GetAllGenresAsync();
        Task<bool> UpdateTrackAsync(Track track);
        Task<bool> SoftDeleteTrackAsync(int trackId);
        Task<bool> HardDeleteTrackAsync(int trackId);
        Task<bool> UpdatePlaylistAsync(int playlistId, int ownerId, string name, bool isPublic, string? coverPath = null);
        Task<bool> RemoveTrackFromPlaylistAsync(int playlistId, int trackId);
        Task<bool> DeletePlaylistAsync(int userId, int playlistId);

    }
}