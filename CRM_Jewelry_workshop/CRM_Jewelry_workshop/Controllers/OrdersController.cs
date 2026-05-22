// Подключаем контекст базы данных (AppDbContext) для работы с данными
using CRM_Jewelry_workshop.Data;
// Подключаем DTO (Data Transfer Object) для передачи данных между клиентом и сервером
using CRM_Jewelry_workshop.DTOs;
// Подключаем модели данных (Order, StatusOrder, User и др.)
using CRM_Jewelry_workshop.Models;
// Подключаем атрибуты авторизации (Authorize) для ограничения доступа по ролям
using Microsoft.AspNetCore.Authorization;
// Подключаем функциональность MVC для создания API-контроллеров
using Microsoft.AspNetCore.Mvc;
// Подключаем Entity Framework Core для асинхронных запросов к БД
using Microsoft.EntityFrameworkCore;

// Пространство имён для контроллеров API
namespace CRM_Jewelry_workshop.Controllers;

// Атрибут Authorize – доступ к контроллеру только для авторизованных пользователей (любая роль)
[Authorize]
// Атрибут ApiController – включает автоматическую валидацию модели, привязку [FromBody] и другие API-фичи
[ApiController]
// Базовый маршрут: все методы будут доступны по /api/orders
[Route("api/[controller]")]
// Наследуемся от BaseController (предположительно, там определены CurrentUserId и CurrentUserRole)
public class OrdersController : BaseController
{
    // Приватное поле для доступа к базе данных
    private readonly AppDbContext _db;

    // Конструктор – внедрение зависимости AppDbContext через DI (Dependency Injection)
    public OrdersController(AppDbContext db) => _db = db;

    // GET: /api/orders/my – получить заказы текущего пользователя (где он клиент)
    [HttpGet("my")]   // Атрибут маршрута "my" добавляется к базовому: /api/orders/my
    public async Task<IActionResult> GetMyOrders()
    {
        // Запрос к таблице Orders с подключением связанных сущностей StatusOrder и Client
        var orders = await _db.Orders
            .Include(o => o.StatusOrder)// Подгружает статус заказа (например, "new", "completed")
            .Include(o => o.Client)// Подгружает данные клиента
            .Where(o => o.ClientId == CurrentUserId) // Фильтруем только заказы текущего пользователя
            .ToListAsync(); // Выполняем запрос асинхронно
        // Возвращает HTTP 200 OK с JSON-массивом заказов
        return Ok(orders);
    }

    // GET: /api/orders/all – получить все заказы (только для admin или manager)
    [Authorize(Roles = "admin,manager")] // Доступ только пользователям с ролями admin или manager
    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders()
    {
        // Запрашивает все заказы с подгрузкой статуса и клиента
        var orders = await _db.Orders
            .Include(o => o.StatusOrder)
            .Include(o => o.Client)
            .ToListAsync();
        return Ok(orders);
    }

    // GET: /api/orders/jeweler – получить заказы, назначенные текущему ювелиру (только для jeweler)
    [Authorize(Roles = "jeweler")]   // Доступ только с ролью jeweler
    [HttpGet("jeweler")]
    public async Task<IActionResult> GetJewelerOrders()
    {
        // Запрашиваем заказы, где JewelerId равен ID текущего пользователя
        var orders = await _db.Orders
            .Include(o => o.StatusOrder)// Подгружаем статус
            .Where(o => o.JewelerId == CurrentUserId) // Только назначенные этому ювелиру
            .ToListAsync();
        return Ok(orders);
    }

    // POST:/api/orders/create – создать новый заказ (доступно любому авторизованному, обычно клиенту)
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
    {
        // Определяем ID клиента в зависимости от роли
        int clientId;
        var role = CurrentUserRole;  // из BaseController (из JWT)

        if (role == "admin" || role == "manager")
        {
            if (dto.ClientId == null)
                return BadRequest("Для создания заказа от имени клиента укажите clientId");
            clientId = dto.ClientId.Value;
        }
        else  // роль "client"
        {
            clientId = CurrentUserId;
        }

        var statusNew = await _db.StatusOrders.FirstOrDefaultAsync(s => s.Name == "new");
        if (statusNew == null) return BadRequest("Статус 'new' не найден");

        var order = new Order
        {
            ClientId = clientId,   // теперь ID клиента правильный
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

        
    

    // Обновляет общую стоимость заказа
    order.TotalCost = totalCost;
        // Сохраняем изменения (обновление TotalCost)
        await _db.SaveChangesAsync();

        // Возвращаем успешный ответ с сообщением и ID созданного заказа
        return Ok(new { message = "Заказ создан", orderId = order.OrderId });
    }

    // PUT: /api/orders/{id}/status – обновить статус заказа (доступно admin, manager, jeweler)
    [Authorize(Roles = "admin,manager,jeweler")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDto dto)
    {
        // Ищем заказ по первичному ключу
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound(); // Если заказ не найден – 404

        // Получаем роль текущего пользователя (из BaseController)
        var role = CurrentUserRole;
        // Если роль "jeweler", проверяем, что заказ назначен именно этому ювелиру
        if (role == "jeweler" && order.JewelerId != CurrentUserId)
            return Forbid(); // Запрещено – 403

        // Находим новый статус по его имени (из dto.Status)
        var newStatus = await _db.StatusOrders.FirstOrDefaultAsync(s => s.Name == dto.Status);
        if (newStatus == null) return BadRequest("Неверный статус");

        // Меняем внешний ключ статуса
        order.StatusOrderId = newStatus.StatusOrderId;
        // Сохраняем изменения
        await _db.SaveChangesAsync();
        return Ok(new { message = "Статус обновлён" });
    }

    // PUT: /api/orders/{id}/assignJeweler – назначает ювелира на заказ (только manager или admin)
    [Authorize(Roles = "manager,admin")]
    [HttpPut("{id}/assignJeweler")]
    public async Task<IActionResult> AssignJeweler(int id, [FromBody] int jewelerId)
    {
        // Ищем заказ
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        // Ищем пользователя по id и подгружаем его роль
        var jeweler = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == jewelerId);
        // Проверяем, что пользователь существует и его роль – "jeweler"
        if (jeweler == null || jeweler.Role?.RoleName != "jeweler")
            return BadRequest("Указанный пользователь не является ювелиром");

        // Назначаем ювелира на заказ
        order.JewelerId = jewelerId;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Ювелир назначен" });
    }

    // PUT: /api/orders/{id}/setDeadline – установить срок выполнения заказа (только jeweler)
    [Authorize(Roles = "jeweler")]
    [HttpPut("{id}/setDeadline")]
    public async Task<IActionResult> SetDeadline(int id, [FromBody] DateTime deadline)
    {
      // Ищем заказ
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
      // Проверяем, что заказ назначен текущему ювелиру
        if (order.JewelerId != CurrentUserId) return Forbid();

      // Устанавливаем срок
        order.Deadline = deadline;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Срок установлен" });
    }

    // PUT: /api/orders/{id}/pay – оплатить заказ (только клиент-владелец заказа)
    [Authorize(Roles = "client")]
    [HttpPut("{id}/pay")]
    public async Task<IActionResult> PayOrder(int id)
    {
     // Ищем заказ
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
     // Проверяем, что текущий пользователь – клиент, создавший этот заказ
        if (order.ClientId != CurrentUserId) return Forbid();

     // Здесь обычно добавляется запись в таблицу Payments, но в текущей реализации просто возвращается сообщение
        return Ok(new { message = "Оплата проведена" });
    }
}