using CRM_Jewelry_workshop.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CRM_Jewelry_workshop.Controllers;

[Authorize(Roles = "admin,manager")]
[ApiController]
[Route("api/[controller]")]
public class MaterialsController : BaseController
{
    private readonly AppDbContext _db;
    public MaterialsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var materials = await _db.Materials.ToListAsync();
        return Ok(materials);
    }
}