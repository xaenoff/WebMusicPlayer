using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using WebMusicPlayer.Components;
using WebMusicPlayer.Data;
using WebMusicPlayer.Services;

namespace WebMusicPlayer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddServerSideBlazor().AddHubOptions(o =>
            {
                o.MaximumReceiveMessageSize = 50 * 1024 * 1024; // 50 MB
            });

            builder.Services.AddRazorPages();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/AccessDenied";
                options.LoginPath = "/login";
                options.ReturnUrlParameter = "ReturnUrl";
            });



            // DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Serv
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IMusicService, MusicService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<PlayerService>();
            builder.Services.AddAntiforgery();


            // Auth
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/Login";
                    options.AccessDeniedPath = "/AccessDenied";
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/music") &&
                    context.Request.Headers.Range.Count > 0)
                {
                    context.Response.Headers.Append("Accept-Ranges", "bytes");
                }
                await next();
            });

            app.UseStaticFiles();
            app.UseStaticFiles();
            app.UseAntiforgery();
            app.UseAntiforgery();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapStaticAssets();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapPost("/logout", async (HttpContext httpContext) =>
            {
                await httpContext.SignOutAsync("Cookies");
                return Results.Redirect("/");
            }).RequireAuthorization();

            app.MapRazorPages();


            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();



            app.Run();
        }
    }
}