// Подключаем контекст базы данных (AppDbContext) для работы с данными
using CRM_Jewelry_workshop.Data;
// Подключаем модели данных (Product – товар/изделие)
using CRM_Jewelry_workshop.Models;
// Подключаем атрибуты авторизации (Authorize) для ограничения доступа
using Microsoft.AspNetCore.Authorization;
// Подключаем функциональность MVC для создания API-контроллеров (ControllerBase, ApiController и др.)
using Microsoft.AspNetCore.Mvc;
// Подключаем Entity Framework Core для асинхронных запросов к БД (ToListAsync, FindAsync, SaveChangesAsync)
using Microsoft.EntityFrameworkCore;

// Пространство имён для контроллеров API
namespace CRM_Jewelry_workshop.Controllers;

// Атрибут ApiController – включает автоматическую валидацию модели, привязку [FromBody] и другие API-фичи
[ApiController]
// Базовый маршрут: все методы будут доступны по /api/products
[Route("api/[controller]")]
// Наследуемся от BaseController (вероятно, там определён CurrentUserId и другие общие методы)
public class ProductsController : BaseController
{
  // Приватное поле для доступа к базе данных
    private readonly AppDbContext _db;

  // Конструктор – внедрение зависимости AppDbContext через DI (Dependency Injection)
    public ProductsController(AppDbContext db) => _db = db;

  // GET: /api/products – получить список всех товаров (доступно всем авторизованным пользователям)
    [HttpGet] // Атрибут указывает, что метод обрабатывает HTTP GET запросы
    public async Task<IActionResult> GetAll()
    {
  // Запрашиваем все товары из таблицы Products асинхронно
        var products = await _db.Products.ToListAsync();
   // Возвращаем HTTP 200 OK с JSON-массивом товаров
        return Ok(products);
    }

    // GET: /api/products/ {id} – получить один товар по его идентификатору (доступно всем авторизованным)
    [HttpGet("{id}")] // {id} – параметр маршрута, подставляется в URL
    public async Task<IActionResult> GetById(int id)
    {
     // Ищет товар по первичному ключу (ProductId)
        var product = await _db.Products.FindAsync(id);
      // Если товар не найден – возвращаем HTTP 404 Not Found
        if (product == null) return NotFound();
      // Если найден – возвращаем 200 OK с JSON-объектом товара
        return Ok(product);
    }

    // POST: /api/products – создать новый товар (только для администраторов)
    [Authorize(Roles = "admin")] // Только пользователи с ролью "admin" могут вызывать этот метод
    [HttpPost]// Атрибут для обработки HTTP POST запросов
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        // Проверяем, что название товара не пустое и не состоит из пробелов
        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest("Название обязательно"); // 400 Bad Request с сообщением об ошибке

        // Добавляем новый товар в контекст (в состояние Added)
        _db.Products.Add(product);
        // Сохраняем изменения в базе данных (вставка новой записи)
        await _db.SaveChangesAsync();
        // Возвращаем 200 OK с созданным товаром (включая сгенерированный ProductId)
        return Ok(product);
    }
}