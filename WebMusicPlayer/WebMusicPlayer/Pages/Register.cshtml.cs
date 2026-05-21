using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebMusicPlayer.Services;

namespace WebMusicPlayer.Pages
{
    [BindProperties]
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        // Поля формы
        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Password { get; set; } = string.Empty;
        [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;
        [BindProperty] public string Code { get; set; } = string.Empty;

        // Состояние шага (1 или 2)
        [BindProperty] public int Step { get; set; } = 1;

        public string Message { get; set; } = string.Empty;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Step == 1)
            {
                //Регистрация
                if (Password != ConfirmPassword)
                {
                    Message = "Пароли не совпадают";
                    return Page();
                }

                var dto = new WebMusicPlayer.Models.DTO.RegisterModel
                {
                    Email = Email,
                    Password = Password,
                    ConfirmPassword = ConfirmPassword
                };

                var success = await _authService.RegisterAsync(dto);

                if (success)
                {
                    Step = 2;
                    Message = " Код отправлен! Смотрите Output → Debug";
                    return Page();
                }
                else
                {
                    Message = "Пользователь с таким email уже существует";
                    return Page();
                }
            }
            else
            {
                // Подтверждение кода
                var success = await _authService.VerifyEmailAsync(Email, Code);

                if (success)
                {
                    Message = " Email подтверждён! Перенаправляем...";
                    return Redirect("/Login");
                }
                else
                {
                    Message = " Неверный код или срок истёк";
                    return Page();
                }
            }
        }
    }
}