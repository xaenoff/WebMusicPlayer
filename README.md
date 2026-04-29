# 🎵 WebMusicPlayer
Веб-плеер на ASP.NET Core 8 + Blazor Server.

## 🚀 Как запустить
1. Установи [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Настрой локальный SQL Server
3. Скопируй `appsettings.example.json` → `appsettings.json` и впиши свои данные (SMTP, строка подключения)
4. Восстанови БД:
   - Вариант А: открой `DatabaseBackup.sql` в SSMS и нажми ▶️ Execute
   - Вариант Б: в SSMS ПКМ по Databases → Restore → выбери `.bak` файл
5. Запусти проект: `dotnet run`
6. Открой `https://localhost:7000`

## 🔐 Безопасность
Файл `appsettings.json` добавлен в `.gitignore`. Не коммить свои пароли в репозиторий!