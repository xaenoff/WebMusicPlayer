using Microsoft.EntityFrameworkCore;
using WebMusicPlayer.Data;
using WebMusicPlayer.Data.Models;
using WebMusicPlayer.Models.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;


namespace WebMusicPlayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContext;

        // Временное хранилище
        private static readonly Dictionary<string, (string Code, DateTime Expires, int UserId)> _verificationCodes = new();
        public AuthService(
            ApplicationDbContext db,
            IEmailService emailService,
            IHttpContextAccessor httpContext)
        {
            _db = db;
            _emailService = emailService;
            _httpContext = httpContext;
        }

        public async Task<bool> RegisterAsync(RegisterModel model)
        {
            // Проверка паролей
            if (model.Password != model.ConfirmPassword)
                return false;

            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null && existingUser.IsEmailConfirmed)
                return false;

            User user;
            if (existingUser != null)
            {
                user = existingUser;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                user.CreatedAt = DateTime.Now;
            }
            else
            {
                user = new User
                {
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "User",
                    IsEmailConfirmed = false,
                    CreatedAt = DateTime.Now
                };
                _db.Users.Add(user);
            }

            await _db.SaveChangesAsync();

            var code = _emailService.GenerateVerificationCode();

            _verificationCodes[model.Email] = (code, DateTime.Now.AddMinutes(15), user.Id);

            try
            {
                await _emailService.SendEmailAsync(
                    model.Email,
                    "Подтверждение регистрации - WebMusicPlayer",
                    $@"
                <h2>Добро пожаловать в WebMusicPlayer!</h2>
                <p>Ваш код подтверждения: <strong style='font-size: 24px; color: #007bff;'>{code}</strong></p>
                <p>Код действителен <strong>15 минут</strong>.</p>
                <p>Если вы не регистрировались, просто проигнорируйте это письмо.</p>
                <hr>
                <p style='color: #666; font-size: 12px;'>WebMusicPlayer © 2026</p>
            "
                );

                Console.WriteLine($" Email отправлен на {model.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Ошибка отправки email: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Не прерываем регистрацию, просто показываем код в консоли как запасной вариант
                Console.WriteLine($" РЕЗЕРВНЫЙ КОД: {code}");
            }

            return true;
        }

        public async Task<bool> VerifyEmailAsync(string email, string code)
        {
         
            var entry = _verificationCodes.FirstOrDefault(x =>
                x.Value.Code == code &&
                x.Value.Expires > DateTime.Now
            );

            if (entry.Key == null)
                return false;

            int userId = entry.Value.UserId;

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.IsEmailConfirmed = true;

            if (string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(email))
            {
                user.Email = email;
            }

            await _db.SaveChangesAsync();

            _verificationCodes.Remove(entry.Key);

            return true;
        }

        public async Task<bool> LoginAsync(LoginModel model)
        {
            Console.WriteLine($"Попытка входа: {model.Email}");

            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                {
                    Console.WriteLine("ользователь не найден");
                    return false;
                }

                Console.WriteLine($"Пользователь найден: {user.Email}");
                Console.WriteLine($"Email подтверждён: {user.IsEmailConfirmed}");

                if (!user.IsEmailConfirmed)
                {
                    Console.WriteLine("Email не подтверждён");
                    return false;
                }

                // Проверка пароля
                var passwordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                Console.WriteLine($"Пароль верен: {passwordValid}");

                if (!passwordValid)
                {
                    Console.WriteLine("Неверный пароль");
                    return false;
                }

                // Создание сессии
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("UserId", user.Id.ToString())
        };

                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                Console.WriteLine("Создаём cookie...");

                if (_httpContext.HttpContext == null)
                {
                    Console.WriteLine("HttpContext is null");
                    return false;
                }

                await _httpContext.HttpContext.SignInAsync(
                    "Cookies",
                    claimsPrincipal,
                    new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    }
                );

                Console.WriteLine("Вход успешен!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в LoginAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            var httpContext = _httpContext.HttpContext;
            if (httpContext != null)
            {
                await httpContext.SignOutAsync("Cookies");
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var email = _httpContext.HttpContext?.User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return null;

            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            return _httpContext.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        }
    }
}