using CRM_Jewelry_workshop.Data;
using CRM_Jewelry_workshop.DTOs;
using CRM_Jewelry_workshop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_Jewelry_workshop.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : BaseController
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var orders = await _db.Orders
            .Include(o => o.StatusOrder)
            .Include(o => o.Client)
            .Where(o => o.ClientId == CurrentUserId)
            .ToListAsync();
        return Ok(orders);
    }

    [Authorize(Roles = "admin,manager")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _db.Orders
            .Include(o => o.StatusOrder)
            .Include(o => o.Client)
            .ToListAsync();
        return Ok(orders);
    }

    [Authorize(Roles = "jeweler")]
    [HttpGet("jeweler")]
    public async Task<IActionResult> GetJewelerOrders()
    {
        var orders = await _db.Orders
            .Include(o => o.StatusOrder)
            .Where(o => o.JewelerId == CurrentUserId)
            .ToListAsync();
        return Ok(orders);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
    {
        var clientId = CurrentUserId;
        var statusNew = await _db.StatusOrders.FirstOrDefaultAsync(s => s.Name == "new");
        if (statusNew == null) return BadRequest("Статус 'new' не найден");

        var order = new Order
        {
            ClientId = clientId,
            StatusOrderId = statusNew.StatusOrderId,
            CreateDate = DateTime.Now,
            TotalCost = 0
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        decimal totalCost = 0;
        foreach (var item in dto.Items)
        {
            if (item.Type == "product")
            {
                var product = await _db.Products.FindAsync(item.Id);
                if (product == null) return BadRequest($"Товар с id {item.Id} не найден");
                totalCost += product.Price * item.Quantity;
            }
            else if (item.Type == "repair")
            {
                totalCost += 3500 * item.Quantity;
            }
            else
            {
                return BadRequest("Неверный тип заказа");
            }
        }

        order.TotalCost = totalCost;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Заказ создан", orderId = order.OrderId });
    }

    [Authorize(Roles = "admin,manager,jeweler")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDto dto)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        var role = CurrentUserRole;
        if (role == "jeweler" && order.JewelerId != CurrentUserId)
            return Forbid();

        var newStatus = await _db.StatusOrders.FirstOrDefaultAsync(s => s.Name == dto.Status);
        if (newStatus == null) return BadRequest("Неверный статус");

        order.StatusOrderId = newStatus.StatusOrderId;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Статус обновлён" });
    }

    [Authorize(Roles = "manager,admin")]
    [HttpPut("{id}/assignJeweler")]
    public async Task<IActionResult> AssignJeweler(int id, [FromBody] int jewelerId)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        var jeweler = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == jewelerId);
        if (jeweler == null || jeweler.Role?.RoleName != "jeweler")
            return BadRequest("Указанный пользователь не является ювелиром");

        order.JewelerId = jewelerId;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Ювелир назначен" });
    }

    [Authorize(Roles = "jeweler")]
    [HttpPut("{id}/setDeadline")]
    public async Task<IActionResult> SetDeadline(int id, [FromBody] DateTime deadline)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        if (order.JewelerId != CurrentUserId) return Forbid();

        order.Deadline = deadline;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Срок установлен" });
    }

    [Authorize(Roles = "client")]
    [HttpPut("{id}/pay")]
    public async Task<IActionResult> PayOrder(int id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        if (order.ClientId != CurrentUserId) return Forbid();
        return Ok(new { message = "Оплата проведена" });
    }
}