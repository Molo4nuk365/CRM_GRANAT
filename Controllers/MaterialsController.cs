using CRM_Granat.Data;
using CRM_Granat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Granat.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager")]
public class MaterialsController : ControllerBase
{
    private readonly AppDbContext _context;
    public MaterialsController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetMaterials()
    {
        var materials = await _context.Materials.ToListAsync();
        return Ok(materials);
    }
}