using BCrypt.Net;
using CRM_Granat.Data;
using CRM_Granat.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Granat.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RepairOptionsController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpGet]
    public async Task<IActionResult> GetRepairOptions()
    {
        var options = await _context.RepairOptions.ToListAsync();
        return Ok(options);
    }
}
