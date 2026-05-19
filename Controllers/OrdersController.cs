using BCrypt.Net;
using CRM_Granat.Data;
using CRM_Granat.DTOs;
using CRM_Granat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRM_Granat.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    public OrdersController(AppDbContext context) => _context = context;

    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    private string GetUserRole() => User.FindFirst(ClaimTypes.Role)?.Value;

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var orders = await _context.Orders
            .Where(o => o.ClientId == GetUserId())
            .ToListAsync();
        return Ok(orders);
    }

    [HttpGet("all")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _context.Orders.ToListAsync();
        return Ok(orders);
    }

    [HttpGet("jeweler")]
    [Authorize(Roles = "jeweler")]
    public async Task<IActionResult> GetJewelerOrders()
    {
        var orders = await _context.Orders
            .Where(o => o.JewelerId == GetUserId())
            .ToListAsync();
        return Ok(orders);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
    {
        int clientId = GetUserId();
        foreach (var item in dto.Items)
        {
            var order = new Order
            {
                ClientId = clientId,
                Type = item.Type,
                Status = "new",
                PaymentStatus = "pending",
                Deadline = DateTime.Now.AddDays(item.Type == "product" ? 14 : 7),
                Total = 0
            };
            if (item.Type == "product")
            {
                var product = await _context.Products.FindAsync(item.Id);
                if (product == null) return BadRequest("Товар не найден");
                order.ProductId = item.Id;
                order.Total = product.Price * item.Quantity;
            }
            else
            {
                var repair = await _context.RepairOptions.FindAsync(item.Id);
                if (repair == null) return BadRequest("Услуга не найдена");
                order.RepairId = item.Id;
                order.Total = repair.Price;
            }
            _context.Orders.Add(order);
        }
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "admin,manager,jeweler")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDto dto)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        var role = GetUserRole();
        if (role == "jeweler" && order.JewelerId != GetUserId())
            return Forbid();

        order.Status = dto.Status;
        if (dto.Status == "completed")
            order.CompletedDate = DateTime.Now;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id}/pay")]
    public async Task<IActionResult> PayOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();
        if (order.ClientId != GetUserId()) return Forbid();
        order.PaymentStatus = "paid";
        await _context.SaveChangesAsync();
        return Ok();
    }
}
