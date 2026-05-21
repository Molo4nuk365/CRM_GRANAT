// Подключаем BCrypt для хеширования паролей
using BCrypt.Net;
// Подключаем контекст БД, DTO и модели
using CRM_Jewelry_workshop.Data;
using CRM_Jewelry_workshop.DTOs;
using CRM_Jewelry_workshop.Models;
// Подключаем авторизацию и MVC
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// Подключаем EF Core для асинхронных запросов
using Microsoft.EntityFrameworkCore;
// Подключаем криптографию для JWT
using Microsoft.IdentityModel.Tokens;
// Подключаем создание JWT-токенов
using System.IdentityModel.Tokens.Jwt;
// Подключаем Claims (утверждения)
using System.Security.Claims;
// Подключаем кодировку строк
using System.Text;

// Пространство имён для контроллеров
namespace CRM_Jewelry_workshop.Controllers;

// Контроллер аутентификации (регистрация, логин, получение текущего пользователя)
// Наследуется от BaseController (чтобы использовать CurrentUserId)
public class AuthController : BaseController
{
    // Поле для работы с БД
    private readonly AppDbContext _db;
    // Поле для доступа к конфигурации (Jwt:Key, Issuer, Audience)
    private readonly IConfiguration _config;

    // Конструктор – внедрение зависимостей
    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // POST: /api/auth/register – регистрация нового пользователя (роль client)
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        // Проверяем, не занят ли логин
        if (await _db.Users.AnyAsync(u => u.Login == dto.Login))
            return BadRequest(new { message = "Логин уже занят" });

        // Ищем роль "client" в БД (должна быть создана SeedData)
        var clientRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "client");
        if (clientRole == null)
            return BadRequest(new { message = "Роль 'client' не найдена. Выполните SeedData." });

        // Создаём нового пользователя
        var user = new User
        {
            Login = dto.Login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Хешируем пароль
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            RoleId = clientRole.RoleId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Регистрация успешна" });
    }

    // POST: /api/auth/login – вход и выдача JWT-токена
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        // Ищем пользователя по логину, включая его роль
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Login == dto.Login);

        // Если пользователь не найден или пароль неверен – 401 Unauthorized
        if (user == null
            || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный логин или пароль" });

        // Генерируем JWT-токен
        var token = GenerateJwtToken(user);
        // Возвращаем данные пользователя и токен
        return Ok(new
        {
            userId = user.UserId,
            fullName = user.FullName,
            roleName = user.Role?.RoleName ?? "client",
            token = token
        });
    }

    // GET: /api/auth/me – получить данные текущего авторизованного пользователя
    [Authorize] // Требует валидный JWT-токен
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        // CurrentUserId – из BaseController (извлекается из токена)
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        if (user == null) return NotFound();

        // Возвращаем информацию о пользователе (без пароля)
        return Ok(new
        {
            user.UserId,
            user.Login,
            user.FullName,
            user.Email,
            user.Phone,
            roleName = user.Role?.RoleName
        });
    }

    // Приватный метод: генерация JWT-токена
    private string GenerateJwtToken(User user)
    {
        // Создаём ключ подписи из строки из конфигурации
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        // Создаём учётные данные для подписи (алгоритм HMAC-SHA256)
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        // Формируем утверждения (claims): ID пользователя, роль, логин
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "client"),
            new Claim(ClaimTypes.Name, user.Login)
        };
        // Создаём JWT-токен с указанными параметрами
        var token = new JwtSecurityToken(
            // Издатель токена (iss) – берётся из конфигурации (appsettings.json)
            issuer: _config["Jwt:Issuer"],
            // Аудитория (aud) – для кого предназначен токен, из конфигурации
            audience: _config["Jwt:Audience"],
            // Утверждения (claims) – данные о пользователе (ID, роль, логин)
            claims: claims,
            // Срок действия токена – истекает через 7 дней с текущего момента
            expires: DateTime.Now.AddDays(7),
            // Учётные данные для подписи (секретный ключ + алгоритм HMAC-SHA256)
            signingCredentials: creds);

        // Преобразуем объект JwtSecurityToken в строку (стандартный JWT формат)
        // JwtSecurityTokenHandler умеет сериализовать токен в Base64Url-кодированные части
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}