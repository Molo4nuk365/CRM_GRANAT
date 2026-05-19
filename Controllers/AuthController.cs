using BCrypt.Net;
using CRM_Granat.Data;
using CRM_Granat.DTOs;
using CRM_Granat.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CRM_Granat.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Login == dto.Login))
            return BadRequest(new { message = "Логин уже занят" });

        var user = new User
        {
            Login = dto.Login,
            PasswordHash = BCrypt.HashPassword(dto.Password),
            Role = "client",
            FullName = dto.FullName,
            Phone = dto.Phone,
            Address = dto.Address
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "OK" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == dto.Login);
        if (user == null || !BCrypt.Verify(dto.Password, user.PasswordHash))  // ← BCrypt
            return Unauthorized(new { message = "Неверный логин или пароль" });

        var token = GenerateJwtToken(user);
        return Ok
            (new
        {
            user.Id,
            user.Login,
            user.Role,
            user.FullName,
            Token = token
        } );
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config ["Jwt:Key"] ));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
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