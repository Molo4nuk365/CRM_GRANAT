using CRM_Jewelry_workshop.Data;
using CRM_Jewelry_workshop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Jewelry_workshop.Controllers;

[Authorize(Roles = "admin,manager,jeweler")]
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

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Material material)
    {
        if (string.IsNullOrWhiteSpace(material.Name))
            return BadRequest("Название обязательно");
        _db.Materials.Add(material);
        await _db.SaveChangesAsync();
        return Ok(material);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Material material)
    {
        var existing = await _db.Materials.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Name = material.Name;
        existing.Unit = material.Unit;
        existing.PricePerUnit = material.PricePerUnit;
        existing.QuantityInStock = material.QuantityInStock;
        existing.Description = material.Description;
        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var material = await _db.Materials.FindAsync(id);
        if (material == null) return NotFound();
        _db.Materials.Remove(material);
        await _db.SaveChangesAsync();
        return Ok();
    }
}