using WebMusicPlayer.Data.Models;
using WebMusicPlayer.Models.DTO;

namespace WebMusicPlayer.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterModel model);
        Task<bool> VerifyEmailAsync(string email, string code);
        Task<bool> LoginAsync(LoginModel model);
        Task LogoutAsync();
        Task<User?> GetCurrentUserAsync();
        Task<bool> IsAuthenticatedAsync();
    }
}