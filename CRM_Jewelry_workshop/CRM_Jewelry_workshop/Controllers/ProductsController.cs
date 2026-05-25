using Microsoft.AspNetCore.Authorization; 
// Для атрибутов [Authorize], ограничивающих доступ к методам по ролям
using Microsoft.AspNetCore.Mvc;           
// Содержит базовые классы и атрибуты для создания контроллеров Web API
using Microsoft.EntityFrameworkCore;     
// Для методов расширения EF Core, таких как ToListAsync, FindAsync
using CRM_Jewelry_workshop.Data;          
// Контекст базы данных (AppDbContext) — точка доступа к таблицам
using CRM_Jewelry_workshop.Models;        
// Модели данных (в данном случае Product)

namespace CRM_Jewelry_workshop.Controllers;

// Контроллер для работы с готовыми изделиями (продуктами) ювелирной мастерской.
// Не имеет атрибута [Authorize] на уровне класса, поэтому методы могут иметь разные уровни доступа.
[ApiController]                          
// Включает автоматическую проверку валидности модели и привязку параметров из тела запроса
[Route("api/[controller]")]              
// Задаёт базовый маршрут: api/Products
public class ProductsController : BaseController 
// Наследует BaseController для доступа к общим свойствам (например, CurrentUserId)
{
    private readonly AppDbContext _db;    
// Контекст базы данных для выполнения операций

// Внедрение зависимости AppDbContext через конструктор (стандартный паттерн Dependency Injection)
    public ProductsController(AppDbContext db) => _db = db;

 // GET api/products — получить список всех изделий.
 // Доступен без авторизации (публичный), поэтому отсутствует [Authorize].
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
 // Возвращаем HTTP 200 OK со списком всех продуктов из базы данных в формате JSON
        Ok(await _db.Products.ToListAsync());

 // GET api/products/{id} — получить одно изделие по идентификатору.
 // Также доступен без ограничений.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
 // Ищем продукт по первичному ключу (быстрый поиск, т.к. используется FindAsync)
        var product = await _db.Products.FindAsync(id);
 // Если продукт не найден — возвращаем 404 Not Found, иначе — 200 OK с объектом
        return product == null ? NotFound() : Ok(product);
    }

 // POST api/products — создать новое изделие.
 // [Authorize(Roles = "admin")] — только администратор может добавлять товары в каталог.
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
// Простейшая валидация: название обязательно.
// IsNullOrWhiteSpace проверяет на null, пустую строку или строку из пробелов.
        if (string.IsNullOrWhiteSpace(product.Name)) return BadRequest("Название обязательно");

// Добавляем новый объект Product в контекст (пока только в памяти)
        _db.Products.Add(product);
// Сохраняем изменения в базе данных, после чего product получит сгенерированный ProductId
        await _db.SaveChangesAsync();
// Возвращаем 200 OK с созданным объектом (можно было бы вернуть 201 Created)
        return Ok(product);
    }

  // PUT api/products/{id} — обновить существующее изделие.
  // Только для администратора.
    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product updated)
    {
  // Проверка, что ID в маршруте совпадает с ID в теле запроса
        if (id != updated.ProductId) return BadRequest("ID не совпадает");

 // Находим существующий продукт в БД по ID
        var existing = await _db.Products.FindAsync(id);
        if (existing == null) return NotFound(); // Если не существует — 404

 // Обновляем все поля существующего объекта значениями из полученного DTO
        existing.Name = updated.Name;
        existing.Description = updated.Description;
        existing.Price = updated.Price;
        existing.ImageUrl = updated.ImageUrl;
        existing.Type = updated.Type;
        existing.Weight = updated.Weight;
        existing.Metal = updated.Metal;
        existing.Stone = updated.Stone;
        existing.Article = updated.Article;

        // Сохраняем изменения в базе данных
        await _db.SaveChangesAsync();
        // Возвращаем обновлённый продукт
        return Ok(existing);
    }

  // DELETE api/products/{id} — удалить изделие.
  // Доступно только администратору.
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
  // Ищем продукт по ID
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound(); // Если не найден — 404

  // Удаляем продукт из контекста (пометка на удаление)
        _db.Products.Remove(product);
 // Сохраняем изменения, физически удаляя запись из БД
        await _db.SaveChangesAsync();
 // Возвращаем 204 No Content — стандартный ответ при успешном удалении без тела
        return NoContent();
    }
}