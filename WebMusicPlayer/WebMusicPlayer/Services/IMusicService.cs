using System.Threading.Tasks;
using WebMusicPlayer.Data.Models;
using WebMusicPlayer.Models;
using WebMusicPlayer.Models.DTO;

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
        Task<int> CreatePlaylistAsync(int userId, string name, List<int> trackIds, string? coverPath = null, bool isPublic = false, bool isSystem = false);
        Task<Playlist?> GetPlaylistByIdAsync(int playlistId);
        Task<Track> CreateTrackAsync(Track track);
        Task<List<Genre>> GetAllGenresAsync();
        Task<bool> UpdateTrackAsync(Track track);
        Task<bool> SoftDeleteTrackAsync(int trackId);
        Task<bool> HardDeleteTrackAsync(int trackId);
        Task<bool> UpdatePlaylistAsync(int playlistId, int ownerId, string name, bool isPublic, string? coverPath = null, bool isAdmin = false);
        Task<bool> RemoveTrackFromPlaylistAsync(int playlistId, int trackId);
        Task<bool> DeletePlaylistAsync(int userId, int playlistId, bool isAdmin = false);
        Task<PagedResult<PublicPlaylistDto>> GetPublicPlaylistsAsync(int page, int pageSize, string? search = null, int? genreId = null, string sortBy = "date", bool strictGenre = false);
        Task<List<PublicPlaylistDto>> GetRandomPlaylistsAsync(int count, bool systemOnly = false);
        Task<List<Genre>> GetPublicPlaylistGenresAsync();
        Task<List<User>> GetAllUsersAsync();
        Task<bool> UpdateUserRoleAsync(int userId, string newRole);
        Task<bool> ToggleUserBlockAsync(int userId);
        Task<(List<Track> Created, List<string> Errors)> CreateTracksBulkAsync(List<Track> tracks);
        Task<bool> AddTrackToPlaylistAsync(int playlistId, int trackId);
        Task<PagedResult<PublicPlaylistDto>> GetSystemPlaylistsAsync(int page, int pageSize, string? search = null, int? genreId = null, string sortBy = "date", bool strictGenre = false);
        Task<bool> SavePlaylistAsync(int userId, int playlistId);
        Task<bool> UnsavePlaylistAsync(int userId, int playlistId);
        Task<bool> IsPlaylistSavedAsync(int userId, int playlistId);
        Task<List<Playlist>> GetSavedPlaylistsAsync(int userId);
        Task<List<Track>> GetRandomTracksAsync(int count);
        Task<PagedResult<Track>> GetAllTracksPagedAsync(int page, int pageSize, string? search = null, int? genreId = null, string sortBy = "name");

    }
}