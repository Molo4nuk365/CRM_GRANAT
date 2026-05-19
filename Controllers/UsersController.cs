using BCrypt.Net;
using CRM_Granat.Data;
using CRM_Granat.DTOs;
using CRM_Granat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Granat.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    public UsersController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new { u.Id, u.Login, u.FullName, u.Role })
            .ToListAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Login == dto.Login))
            return BadRequest("Логин занят");

        var user = new User
        {
            Login = dto.Login,
            PasswordHash = BCrypt.HashPassword(dto.Password),
            Role = dto.Role ?? "client",
            FullName = dto.FullName,
            Phone = dto.Phone,
            Address = dto.Address,
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok();
    }
}