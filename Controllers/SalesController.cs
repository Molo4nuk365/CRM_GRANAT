using CRM_Granat.Data;
using CRM_Granat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GranatCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class SalesController : ControllerBase
{
    private readonly AppDbContext _context;
    public SalesController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetSalesHistory(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? clientId,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount)
    {
        var query = _context.Orders.Where(o => o.Status == "completed");

        if (from.HasValue)
            query = query.Where(o => o.CompletedDate >= from.Value);
        if (to.HasValue)
            query = query.Where(o => o.CompletedDate <= to.Value);
        if (clientId.HasValue)
            query = query.Where(o => o.ClientId == clientId.Value);
        if (minAmount.HasValue)
            query = query.Where(o => o.Total >= minAmount.Value);
        if (maxAmount.HasValue)
            query = query.Where(o => o.Total <= maxAmount.Value);

        var sales = await query.ToListAsync();
        var total = sales.Sum(s => s.Total);
        var clientNames = await _context.Users.ToDictionaryAsync(u => u.Id, u => u.FullName);

        var result = sales.Select(s => new
        {
            s.Id,
            ClientName = clientNames.GetValueOrDefault(s.ClientId, "Неизвестно"),
            s.Total,
            s.CompletedDate
        });

        return Ok(new { orders = result, total });
    }
}
