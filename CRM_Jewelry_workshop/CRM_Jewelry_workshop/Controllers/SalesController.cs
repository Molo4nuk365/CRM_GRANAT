using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM_Jewelry_workshop.Data;

namespace CRM_Jewelry_workshop.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/[controller]")]
public class SalesController : BaseController
{
    private readonly AppDbContext _db;
    public SalesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetSales([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _db.Orders
            .Include(o => o.StatusOrder)
            .Where(o => o.StatusOrder!.Name == "completed");

        if (from.HasValue) query = query.Where(o => o.CreateDate >= from);
        if (to.HasValue) query = query.Where(o => o.CreateDate <= to);

        var orders = await query.ToListAsync();
        var total = orders.Sum(o => o.TotalCost);
        return Ok(new { orders, total });
    }
}