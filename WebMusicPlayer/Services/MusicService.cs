using Microsoft.EntityFrameworkCore;
using WebMusicPlayer.Data;
using WebMusicPlayer.Data.Models;

namespace WebMusicPlayer.Services
{
    public class MusicService : IMusicService
    {
        private readonly ApplicationDbContext _db;

        public MusicService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Track>> GetAllTracksAsync()
        {
            return await _db.Tracks
                .Include(t => t.Genre)
                .Where(t => t.IsAvailable)
                .ToListAsync();
        }

        public async Task<Track?> GetTrackByIdAsync(int id)
        {
            Console.WriteLine($"[MusicService] Запрос трека ID={id}");

            var track = await _db.Tracks
                .Include(t => t.Genre)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (track == null)
                Console.WriteLine($"[MusicService] Трек ID={id} не найден в БД");
            else
                Console.WriteLine($"[MusicService] Трек найден: {track.Title}, IsAvailable={track.IsAvailable}");

            return track;
        }

        public async Task<List<Track>> GetFavoriteTracksAsync(int userId)
        {
            return await _db.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Track)
                    .ThenInclude(t => t.Genre)
                .Select(f => f.Track)
                .Where(t => t != null && t.IsAvailable)
                .ToListAsync();
        }

        public async Task<List<Playlist>> GetAllPlaylistsAsync()
        {
            return await _db.Playlists
                .Include(p => p.Owner)
                .Include(p => p.Playlist_track)
                    .ThenInclude(pt => pt.Track)
                .Where(p => p.IsPublic || p.IsSystem)
                .ToListAsync();
        }

        public async Task<List<Track>> SearchTracksAsync(string query)
        {
            return await _db.Tracks
                .Include(t => t.Genre)
                .Where(t => t.IsAvailable &&
                      (t.Title.Contains(query) || t.Artist.Contains(query)))
                .ToListAsync();
        }

        public async Task<bool> AddToFavoriteAsync(int userId, int trackId)
        {
            try
            {
                var exists = await _db.Favorites
                    .AnyAsync(f => f.UserId == userId && f.TrackId == trackId);

                if (exists)
                    return false;

                _db.Favorites.Add(new Favorite { UserId = userId, TrackId = trackId });
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveFromFavoriteAsync(int userId, int trackId)
        {
            try
            {
                var favorite = await _db.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.TrackId == trackId);

                if (favorite == null)
                    return false;

                _db.Favorites.Remove(favorite);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsFavoriteAsync(int userId, int trackId)
        {
            return await _db.Favorites
                .AnyAsync(f => f.UserId == userId && f.TrackId == trackId);
        }

        public async Task<int> CreatePlaylistAsync(int userId, string name, List<int> trackIds, string? coverPath = null, bool isPublic = false)
        {
            var playlist = new Playlist
            {
                Name = name,
                OwnerId = userId,
                CoverPath = coverPath,
                IsPublic = isPublic, 
                IsSystem = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Playlists.Add(playlist);
            await _db.SaveChangesAsync();

            foreach (var trackId in trackIds)
            {
                var track = await _db.Tracks.FirstOrDefaultAsync(t => t.Id == trackId && t.IsAvailable);
                if (track != null)
                {
                    _db.Playlist_track.Add(new Playlist_track
                    {
                        PlaylistId = playlist.Id,
                        TrackId = trackId,
                        AddedAt = DateTime.UtcNow
                    });
                }
            }

            await _db.SaveChangesAsync();
            return playlist.Id;
        }
        public async Task<List<Playlist>> GetUserPlaylistsAsync(int userId)
        {
            return await _db.Playlists
                .Include(p => p.Owner)  
                .Include(p => p.Playlist_track) 
                    .ThenInclude(pt => pt.Track) 
                .Where(p => p.OwnerId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Track>> GetPlaylistTracksAsync(int playlistId)
        {
            return await _db.Playlist_track
                .Where(pt => pt.PlaylistId == playlistId)
                .Include(pt => pt.Track)
                    .ThenInclude(t => t.Genre)
                .Select(pt => pt.Track)
                .Where(t => t != null && t.IsAvailable)
                .ToListAsync();
        }

        public async Task<bool> DeletePlaylistAsync(int userId, int playlistId)
        {
            var playlist = await _db.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.OwnerId == userId);

            if (playlist == null) return false;

            // Сначала удаляем связи с треками (из за каскада мб и не надо)
            var links = await _db.Playlist_track.Where(pt => pt.PlaylistId == playlistId).ToListAsync();
            _db.Playlist_track.RemoveRange(links);

            _db.Playlists.Remove(playlist);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<Playlist?> GetPlaylistByIdAsync(int playlistId)
        {
            return await _db.Playlists
                .Include(p => p.Owner)
                .Include(p => p.Playlist_track)
                    .ThenInclude(pt => pt.Track)
                .FirstOrDefaultAsync(p => p.Id == playlistId);
        }

        public async Task<Track> CreateTrackAsync(Track track)
        {
            _db.Tracks.Add(track);
            await _db.SaveChangesAsync();
            return track;
        }

        public async Task<List<Genre>> GetAllGenresAsync()
        {
            return await _db.Genres.OrderBy(g => g.Name).ToListAsync();
        }

        public async Task<bool> UpdateTrackAsync(Track track)
        {
            try
            {
                var existing = await _db.Tracks.FindAsync(track.Id);
                if (existing == null) return false;

                existing.Title = track.Title;
                existing.Artist = track.Artist;
                existing.GenreId = track.GenreId;


                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SoftDeleteTrackAsync(int trackId)
        {
            try
            {
                var track = await _db.Tracks.FindAsync(trackId);
                if (track == null) return false;

                // Мягкое удаление
                track.IsAvailable = false;

                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HardDeleteTrackAsync(int trackId)
        {
            try
            {
                var track = await _db.Tracks.FindAsync(trackId);
                if (track == null) return false;

                _db.Tracks.Remove(track);
                await _db.SaveChangesAsync();



                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePlaylistAsync(int playlistId, int ownerId, string name, bool isPublic, string? coverPath = null)
        {
            var playlist = await _db.Playlists.FirstOrDefaultAsync(p => p.Id == playlistId && p.OwnerId == ownerId);
            if (playlist == null) return false;

            playlist.Name = name;
            playlist.IsPublic = isPublic;
            if (!string.IsNullOrEmpty(coverPath))
            {
                playlist.CoverPath = coverPath;
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTrackFromPlaylistAsync(int playlistId, int trackId)
        {
            var link = await _db.Playlist_track
                .FirstOrDefaultAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId);

            if (link == null) return false;

            _db.Playlist_track.Remove(link);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}