using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using CRM_Jewelry_workshop.Data;
using CRM_Jewelry_workshop.DTOs;
using CRM_Jewelry_workshop.Models;
using System.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace CRM_Jewelry_workshop.Controllers;

public class AuthController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Login == dto.Login))
            return BadRequest(new { message = "Логин уже занят" });

        var clientRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "client");
        if (clientRole == null)
            return BadRequest(new { message = "Роль 'client' не найдена. Выполните SeedData." });

        var user = new User
        {
            Login = dto.Login,
            PasswordHash = BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            RoleId = clientRole.RoleId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Регистрация успешна" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Login == dto.Login);

        if (user == null
            || !BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный логин или пароль" });

        var token = GenerateJwtToken(user);
        return Ok(new
        {
            userId = user.UserId,
            fullName = user.FullName,
            roleName = user.Role?.RoleName ?? "client",
            token = token
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        if (user == null) return NotFound();
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

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "client"),
            new Claim(ClaimTypes.Name, user.Login)
        };
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
