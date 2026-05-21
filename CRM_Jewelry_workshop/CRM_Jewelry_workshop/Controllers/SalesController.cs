// Подключаем атрибуты авторизации (Authorize) для ограничения доступа по ролям
using Microsoft.AspNetCore.Authorization;
// Подключаем функциональность MVC для создания API-контроллеров (ControllerBase, ApiController и др.)
using Microsoft.AspNetCore.Mvc;
// Подключаем Entity Framework Core для асинхронных запросов к БД (Include, Where, ToListAsync и др.)
using Microsoft.EntityFrameworkCore;
// Подключаем наш контекст базы данных (AppDbContext) и модели данных
using CRM_Jewelry_workshop.Data;

// Пространство имён для контроллеров API
namespace CRM_Jewelry_workshop.Controllers;

// Атрибут Authorize – доступ к этому контроллеру только для пользователей с ролью "admin"
[Authorize(Roles = "admin")]
// Атрибут ApiController – включает автоматическую валидацию модели, привязку [FromBody] и другие API-фичи
[ApiController]
// Базовый маршрут: все методы будут доступны по /api/sales
[Route("api/[controller]")]
// Наследуемся от BaseController (вероятно, там определён CurrentUserId и другие общие методы)
public class SalesController : BaseController
{
// Приватное поле для доступа к базе данных
    private readonly AppDbContext _db;

// Конструктор – внедрение зависимости AppDbContext через DI (Dependency Injection)
    public SalesController(AppDbContext db) => _db = db;

 //  Получаем список выполненных заказов (продаж) за период
    [HttpGet] // Атрибут указывает, что метод обрабатывает HTTP GET запросы
    // Параметры from и to извлекаются из строки запроса ([FromQuery])
    public async Task<IActionResult> GetSales([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        // Формируем базовый запрос к таблице Orders с подключением связанной сущности StatusOrder
        var query = _db.Orders
            .Include(o => o.StatusOrder)// Подгружаем статус заказа
            .Where(o => o.StatusOrder!.Name == "completed"); // Фильтруем только выполненные заказы (продажи)

        // Если параметр from передан (имеет значение), добавляем условие: дата создания >= from
        if (from.HasValue) query = query.Where(o => o.CreateDate >= from);

        // Если параметр to передан, добавляем условие: дата создания <= to
        if (to.HasValue) query = query.Where(o => o.CreateDate <= to);

        // Выполняем запрос асинхронно и получаем список заказов
        var orders = await query.ToListAsync();

        // Вычисляем общую сумму всех найденных заказов (суммируем TotalCost)
        var total = orders.Sum(o => o.TotalCost);

        // Возвращаем HTTP 200 OK с объектом, содержащим список заказов и общую сумму
        return Ok(new { orders, total });
    }
}