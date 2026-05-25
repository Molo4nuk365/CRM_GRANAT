using Microsoft.AspNetCore.Authorization; 
// Для атрибутов [Authorize], ограничивающих доступ к контроллеру и действиям
using Microsoft.AspNetCore.Mvc;           
// Базовые классы и атрибуты для контроллеров Web API
using Microsoft.EntityFrameworkCore;     
// Для использования Include, ToListAsync и других методов EF Core
using CRM_Jewelry_workshop.Data;          
// Контекст базы данных AppDbContext

namespace CRM_Jewelry_workshop.Controllers;

// Контроллер доступен только администраторам (роль "admin").
// Использует атрибут [Authorize] на уровне класса, запрещая доступ другим ролям.
[Authorize(Roles = "admin")]
[ApiController]                      
// Автоматическая валидация модели и привязка параметров
[Route("api/[controller]")]          
// Маршрут: api/Sales
public class SalesController : BaseController 
    // Наследует BaseController, где может быть CurrentUserId и т.д.
{
    private readonly AppDbContext _db; 
    // Контекст базы данных для выполнения запросов

    // Внедрение зависимости через конструктор (DI)
    public SalesController(AppDbContext db) => _db = db;

    // GET api/sales?from=...&to=...
    // Возвращает список выполненных заказов за указанный период (или без фильтра, если даты не переданы).
    [HttpGet]
    public async Task<IActionResult> GetSales([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        // Формируем запрос к таблице заказов с подгрузкой статуса заказа
        var query = _db.Orders
            .Include(o => o.StatusOrder)// Жадная загрузка статуса, чтобы проверить его название
            .Where(o => o.StatusOrder!.Name == "Выполнен"); 
        // Отбираем только заказы со статусом "Выполнен"
        // "!" — утверждение, что StatusOrder не null (мы уверены после Include)

        // Если передан параметр from, добавляем фильтр по дате создания заказа "с"
        if (from.HasValue) query = query.Where(o => o.CreateDate >= from);
        // Если передан параметр to, добавляем фильтр по дате создания заказа "до"
        if (to.HasValue) query = query.Where(o => o.CreateDate <= to);

        // Выполняем запрос к БД и получаем список заказов
        var orders = await query.ToListAsync();

        // Считаем общую сумму всех отфильтрованных заказов
        var total = orders.Sum(o => o.TotalCost);

        // Возвращаем HTTP 200 OK с объектом, содержащим список заказов и итоговую сумму
        return Ok(new { orders, total });
    }
}