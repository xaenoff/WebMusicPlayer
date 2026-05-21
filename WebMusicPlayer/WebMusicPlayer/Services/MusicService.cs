using Microsoft.EntityFrameworkCore;
using WebMusicPlayer.Data;
using WebMusicPlayer.Data.Models;
using WebMusicPlayer.Models;
using WebMusicPlayer.Models.DTO;

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

        public async Task<int> CreatePlaylistAsync(int userId, string name, List<int> trackIds, string? coverPath = null, bool isPublic = false, bool isSystem = false)
        {
            var playlist = new Playlist
            {
                Name = name,
                OwnerId = isSystem ? null : userId,
                CoverPath = coverPath,
                IsPublic = isPublic,
                IsSystem = isSystem,
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

        public async Task<bool> DeletePlaylistAsync(int userId, int playlistId, bool isAdmin = false)
        {
            var playlist = await _db.Playlists.FirstOrDefaultAsync(p => p.Id == playlistId);
            if (playlist == null) return false;

            bool canDelete = playlist.OwnerId == userId || (playlist.IsSystem && isAdmin);
            if (!canDelete) return false;

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
                existing.Lyrics = track.Lyrics;
                existing.CoverPath = track.CoverPath;
                existing.IsAvailable = track.IsAvailable;


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

        public async Task<bool> UpdatePlaylistAsync(int playlistId, int ownerId, string name, bool isPublic, string? coverPath = null, bool isAdmin = false)
        {
            var playlist = await _db.Playlists.FirstOrDefaultAsync(p => p.Id == playlistId);
            if (playlist == null) return false;

            bool canEdit = playlist.OwnerId == ownerId || (playlist.IsSystem && isAdmin);
            if (!canEdit) return false;

            playlist.Name = name;
            playlist.IsPublic = isPublic;
            if (!string.IsNullOrEmpty(coverPath)) playlist.CoverPath = coverPath;

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

        public async Task<PagedResult<PublicPlaylistDto>> GetPublicPlaylistsAsync(
    int page, int pageSize, string? search = null, int? genreId = null,
    string sortBy = "date", bool strictGenre = false)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var query = _db.Playlists
                .Where(p => p.IsPublic && !p.IsSystem)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(s));
            }

            if (genreId.HasValue)
            {
                if (strictGenre)
                {
                    query = query.Where(p => p.Playlist_track != null &&
                        p.Playlist_track.Any(pt => pt.Track != null && pt.Track.GenreId == genreId.Value) &&
                        p.Playlist_track.All(pt => pt.Track == null || pt.Track.GenreId == genreId.Value));
                }
                else
                {
                    query = query.Where(p => p.Playlist_track != null &&
                        p.Playlist_track.Any(pt => pt.Track != null && pt.Track.GenreId == genreId.Value));
                }
            }

            var totalCount = await query.CountAsync();

            query = sortBy.ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name),
                "tracks" => query.OrderByDescending(p => p.Playlist_track.Count),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var playlists = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Owner)
                .Include(p => p.Playlist_track)
                    .ThenInclude(pt => pt.Track)
                .ToListAsync();

            var dtos = playlists.Select(p => new PublicPlaylistDto
            {
                Id = p.Id,
                Name = p.Name,
                OwnerEmail = p.Owner?.Email ?? "Unknown",
                IsSystem = p.IsSystem,
                CreatedAt = p.CreatedAt,
                CoverPath = p.CoverPath,
                TrackCount = p.Playlist_track?.Count ?? 0,
                FirstTrackCoverPath = p.Playlist_track?.FirstOrDefault()?.Track?.CoverPath
            }).ToList();

            return PagedResult<PublicPlaylistDto>.Create(dtos, totalCount, page, pageSize);
        }

        public async Task<List<PublicPlaylistDto>> GetRandomPlaylistsAsync(int count, bool systemOnly = false)
        {
            var query = _db.Playlists.AsNoTracking();

            if (systemOnly)
            {
                query = query.Where(p => p.IsSystem == true);
            }
            else
            {
                query = query.Where(p => p.IsPublic == true && p.IsSystem == false);
            }

            var playlists = await query
                .OrderBy(p => Guid.NewGuid())
                .Take(count)
                .Include(p => p.Owner)
                .Include(p => p.Playlist_track)
                    .ThenInclude(pt => pt.Track)
                .ToListAsync();

            return playlists.Select(p => new PublicPlaylistDto
            {
                Id = p.Id,
                Name = p.Name,
                OwnerEmail = p.IsSystem ? "System" : p.Owner?.Email ?? "Unknown",
                IsSystem = p.IsSystem,
                CreatedAt = p.CreatedAt,
                CoverPath = p.CoverPath,
                TrackCount = p.Playlist_track?.Count ?? 0,
                FirstTrackCoverPath = p.Playlist_track?.FirstOrDefault()?.Track?.CoverPath
            }).ToList();
        }

        public async Task<List<Genre>> GetPublicPlaylistGenresAsync()
        {
            var genres = await (from p in _db.Playlists
                                where p.IsPublic || p.IsSystem
                                join pt in _db.Playlist_track on p.Id equals pt.PlaylistId
                                join t in _db.Tracks on pt.TrackId equals t.Id
                                join g in _db.Genres on t.GenreId equals g.Id
                                select g)
                .Distinct()
                .OrderBy(g => g.Name)
                .ToListAsync();

            return genres;
        }

        // DTO для жанра
        public class GenreDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        // ==============АДМИНКА==================

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _db.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            user.Role = newRole;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleUserBlockAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsBlocked = !user.IsBlocked;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<(List<Track> Created, List<string> Errors)> CreateTracksBulkAsync(List<Track> tracks)
        {
            var created = new List<Track>();
            var errors = new List<string>();

            foreach (var track in tracks)
            {
                try
                {
                    _db.Tracks.Add(track);
                    await _db.SaveChangesAsync(); // Сохраняем каждый трек отдельно, чтобы ошибка на одном не отменяла остальные
                    created.Add(track);
                }
                catch (Exception ex)
                {
                    errors.Add($"Трек '{track.Title}' не сохранён: {ex.Message}");
                }
            }

            return (created, errors);
        }

        public async Task<bool> AddTrackToPlaylistAsync(int playlistId, int trackId)
        {
            var exists = await _db.Playlist_track.AnyAsync(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId);
            if (exists) return false;

            _db.Playlist_track.Add(new Playlist_track
            {
                PlaylistId = playlistId,
                TrackId = trackId,
                AddedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResult<PublicPlaylistDto>> GetSystemPlaylistsAsync(
    int page, int pageSize, string? search = null, int? genreId = null,
    string sortBy = "date", bool strictGenre = false)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var query = _db.Playlists
                .Where(p => p.IsSystem)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(s));
            }

            if (genreId.HasValue)
            {
                if (strictGenre)
                {
                    query = query.Where(p => p.Playlist_track != null &&
                        p.Playlist_track.Any(pt => pt.Track != null && pt.Track.GenreId == genreId.Value) &&
                        p.Playlist_track.All(pt => pt.Track == null || pt.Track.GenreId == genreId.Value));
                }
                else
                {
                    query = query.Where(p => p.Playlist_track != null &&
                        p.Playlist_track.Any(pt => pt.Track != null && pt.Track.GenreId == genreId.Value));
                }
            }

            var totalCount = await query.CountAsync();

            query = sortBy.ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name),
                "tracks" => query.OrderByDescending(p => p.Playlist_track.Count),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var playlists = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Owner)
                .Include(p => p.Playlist_track)
                    .ThenInclude(pt => pt.Track)
                .ToListAsync();

            var dtos = playlists.Select(p => new PublicPlaylistDto
            {
                Id = p.Id,
                Name = p.Name,
                OwnerEmail = "System",
                IsSystem = true,
                CreatedAt = p.CreatedAt,
                CoverPath = p.CoverPath,
                TrackCount = p.Playlist_track?.Count ?? 0,
                FirstTrackCoverPath = p.Playlist_track?.FirstOrDefault()?.Track?.CoverPath
            }).ToList();

            return PagedResult<PublicPlaylistDto>.Create(dtos, totalCount, page, pageSize);
        }

        public async Task<bool> SavePlaylistAsync(int userId, int playlistId)
        {
            try
            {
                var exists = await _db.SavedPlaylists.AnyAsync(sp => sp.UserId == userId && sp.PlaylistId == playlistId);
                if (exists) return false;

                var playlist = await _db.Playlists.FindAsync(playlistId);
                if (playlist == null) return false;

                _db.SavedPlaylists.Add(new SavedPlaylist { UserId = userId, PlaylistId = playlistId });
                await _db.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> UnsavePlaylistAsync(int userId, int playlistId)
        {
            try
            {
                var saved = await _db.SavedPlaylists.FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PlaylistId == playlistId);
                if (saved == null) return false;

                _db.SavedPlaylists.Remove(saved);
                await _db.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> IsPlaylistSavedAsync(int userId, int playlistId)
        {
            return await _db.SavedPlaylists.AnyAsync(sp => sp.UserId == userId && sp.PlaylistId == playlistId);
        }

        public async Task<List<Playlist>> GetSavedPlaylistsAsync(int userId)
        {
            return await _db.SavedPlaylists
                .Where(sp => sp.UserId == userId)
                .Include(sp => sp.Playlist)
                    .ThenInclude(p => p.Owner)
                .Include(sp => sp.Playlist.Playlist_track)
                    .ThenInclude(pt => pt.Track)
                .Select(sp => sp.Playlist)
                .OrderByDescending(sp => sp.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Track>> GetRandomTracksAsync(int count)
        {
            var allTracks = await _db.Tracks
                .Include(t => t.Genre)
                .Where(t => t.IsAvailable)
                .ToListAsync();

            var random = new Random();
            return allTracks
                .OrderBy(t => random.Next())
                .Take(count)
                .ToList();
        }

        public async Task<PagedResult<Track>> GetAllTracksPagedAsync(int page, int pageSize, string? search = null, int? genreId = null, string sortBy = "name")
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var query = _db.Tracks
                .Include(t => t.Genre)
                .Where(t => t.IsAvailable)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(t => t.Title.ToLower().Contains(s) || t.Artist.ToLower().Contains(s));
            }

            if (genreId.HasValue)
            {
                query = query.Where(t => t.GenreId == genreId.Value);
            }

            var totalCount = await query.CountAsync();

            query = sortBy.ToLower() switch
            {
                "name" => query.OrderBy(t => t.Title),
                "artist" => query.OrderBy(t => t.Artist),
                "date" => query.OrderByDescending(t => t.Id),
                "duration" => query.OrderByDescending(t => t.DurationSeconds),
                _ => query.OrderBy(t => t.Title)
            };

            var tracks = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PagedResult<Track>.Create(tracks, totalCount, page, pageSize);
        }
    }
}