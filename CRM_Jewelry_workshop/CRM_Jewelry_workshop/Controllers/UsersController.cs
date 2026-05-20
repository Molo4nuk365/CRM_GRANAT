using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM_Jewelry_workshop.Data;

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
}