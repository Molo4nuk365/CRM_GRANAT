using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM_Jewelry_workshop.Data;
using CRM_Jewelry_workshop.DTO;



namespace CRM_Jewelry_workshop.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .Include(u => u.Role)
            .Select(u => new { u.UserId, u.Login, u.FullName, RoleName = u.Role!.RoleName })
            .ToListAsync();
        return Ok(users);
    }
    [HttpPut("{id}/role")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] RoleChangeDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
        if (role == null) return BadRequest("Роль не найдена");
        user.RoleId = role.RoleId;
        await _db.SaveChangesAsync();
        return Ok();
    }

}