using BCrypt.Net;                      
// Библиотека BCrypt для хеширования и проверки паролей (методы HashPassword, Verify)
using CRM_Jewelry_workshop.Data;       
// Контекст базы данных (AppDbContext)
using CRM_Jewelry_workshop.DTO;        
// DTO-классы (RegisterDto, LoginDto) для передачи данных аутентификации
using CRM_Jewelry_workshop.Models;     
// Модели сущностей, в том числе User и Role
using Microsoft.AspNetCore.Authorization; 
// Атрибут [Authorize] для защиты методов
using Microsoft.AspNetCore.Mvc;          
// Базовые классы для контроллеров (ApiController, Route, IActionResult и др.)
using Microsoft.EntityFrameworkCore;     
// Методы расширения EF Core: Include, FirstOrDefaultAsync, AnyAsync
using Microsoft.IdentityModel.Tokens;    
// Классы для работы с токенами: SymmetricSecurityKey, SigningCredentials
using System.IdentityModel.Tokens.Jwt;   
// Создание и запись JWT-токенов (JwtSecurityToken, JwtSecurityTokenHandler)
using System.Security.Claims;            
// Утверждения (claims) для токена: ClaimTypes.NameIdentifier, ClaimTypes.Role
using System.Text;                       
// Кодировка строки в байты для ключа безопасности (Encoding.UTF8)

namespace CRM_Jewelry_workshop.Controllers;

// Контроллер аутентификации — регистрация, вход, получение информации о текущем пользователе.
[ApiController]                    
// Автоматическая валидация модели и привязка параметров из тела запроса
[Route("api/[controller]")]        
// Базовый маршрут: api/Auth
public class AuthController : BaseController  
    // Наследует свойства CurrentUserId, CurrentUserRole
{
    private readonly AppDbContext _db;          
    // Контекст базы данных для работы с пользователями и ролями
    private readonly IConfiguration _config;    
    // Доступ к настройкам приложения (appsettings.json), в том числе к JWT-секции

    // Конструктор с внедрением зависимостей (DbContext и IConfiguration)
    // Используется кортежное присваивание для лаконичности
    public AuthController(AppDbContext db, IConfiguration config) => (_db, _config) = (db, config);

    // POST api/auth/register — регистрация нового пользователя.
    // Доступен без аутентификации (нет [Authorize]).
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        // Проверяем уникальность логина: если такой уже существует, возвращаем ошибку
        if (await _db.Users.AnyAsync(u => u.Login == dto.Login))
            return BadRequest(new { message = "Логин уже занят" });

        // Находим роль "client", которую получает каждый новый зарегистрированный пользователь
        var clientRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "client");
        if (clientRole == null) return BadRequest("Роль 'client' не найдена");

        // Создаём объект User на основе данных из DTO
        var user = new User
        {
            Login = dto.Login,
            // Хешируем пароль с помощью BCrypt перед сохранением (никогда не храним пароли в открытом виде)
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            RoleId = clientRole.RoleId   
            // Присваиваем роль "client"
        };

        // Добавляем нового пользователя в контекст
        _db.Users.Add(user);
        // Сохраняем изменения в базе данных (после этого user получит UserId)
        await _db.SaveChangesAsync();

        // Возвращаем сообщение об успешной регистрации
        return Ok(new { message = "Регистрация успешна" });
    }

    // POST api/auth/login — вход в систему, получение JWT-токена.
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
     // Ищем пользователя по логину и сразу подгружаем его роль (понадобится для токена)
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Login == dto.Login);

     // Если пользователь не найден или пароль не совпадает — возвращает 401 Unauthorized
     // BCrypt.Verify сравнивает переданный пароль с сохранённым хешем
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный логин или пароль" });

     // Генерируем JWT-токен для успешно аутентифицированного пользователя
        var token = GenerateJwtToken(user);

     // Возвращаем данные пользователя и токен
        return Ok(new
        {
            userId = user.UserId,
            fullName = user.FullName,
            roleName = user.Role?.RoleName ?? "client", // Если роль не загружена, подставляем "client"
            token
        });
    }

    // GET api/auth/me — получает информацию о текущем авторизованном пользователе.
    // Требуется аутентификация ([Authorize] без указания ролей — любой авторизованный пользователь).
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
   // Находим пользователя по его ID, полученному из JWT-токена (через CurrentUserId базового контроллера)
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        if (user == null) return NotFound(); // Если вдруг пользователь не найден — 404

  // Возвращаем основную информацию о пользователе, включая роль
        return Ok (new
        {
            user.UserId,
            user.Login,
            user.FullName,
            user.Email,
            user.Phone,
            roleName = user.Role?.RoleName
        });
    }

    // Приватный метод генерации JWT-токена.
    // Принимает объект User и возвращает подписанный JWT в виде строки.
    private string GenerateJwtToken(User user)
    {
        // Создаём ключ безопасности на основе секретного ключа из конфигурации (appsettings.json, секция "Jwt:Key")
   var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        // Указываем алгоритм подписи — HMAC-SHA256
   var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

     // Формируем набор утверждений (claims), которые будут встроены в токен:
        var claims = new[]
        {
            // Уникальный идентификатор пользователя (для BaseController.CurrentUserId)
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            // Роль пользователя (для авторизации на основе ролей)
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "client"),
            // Имя пользователя (логин) — дополнительная информация
            new Claim(ClaimTypes.Name, user.Login)
        };

        // Создаём объект JwtSecurityToken со всеми параметрами:
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],           
            // Кто выпустил токен
            audience: _config["Jwt:Audience"],        
            // Для кого предназначен токен
            claims: claims,                           
            // Утверждения
            expires: DateTime.Now.AddDays(30),         
            // Срок действия — 30 дней
            signingCredentials: creds);               
            // Ключ и алгоритм подписи

        // Сериализуем токен в строку и возвращаем его
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}