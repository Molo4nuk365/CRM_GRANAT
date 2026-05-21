// Подключаем контекст базы данных (AppDbContext) для работы с данными
using CRM_Jewelry_workshop.Data;
// Подключаем модели данных (Material – материал, используемый в производстве)
using CRM_Jewelry_workshop.Models;
// Подключаем атрибуты авторизации (Authorize) для ограничения доступа по ролям
using Microsoft.AspNetCore.Authorization;
// Подключаем функциональность MVC для создания API-контроллеров
using Microsoft.AspNetCore.Mvc;
// Подключаем Entity Framework Core для асинхронных запросов к БД
using Microsoft.EntityFrameworkCore;

// Пространство имён для контроллеров API
namespace CRM_Jewelry_workshop.Controllers;

// Атрибут Authorize – доступ к контроллеру только для пользователей с ролями: admin, manager, jeweler
[Authorize(Roles = "admin,manager,jeweler")]
// Атрибут ApiController – включает автоматическую валидацию модели, привязку [FromBody] и другие API-фичи
[ApiController]
// Базовый маршрут: все методы будут доступны по /api/materials
[Route("api/[controller]")]
// Наследуемся от BaseController (предположительно, там определены CurrentUserId, CurrentUserRole и общие методы)
public class MaterialsController : BaseController
{
    // Приватное поле для доступа к базе данных
    private readonly AppDbContext _db;

    // Конструктор – внедрение зависимости AppDbContext через DI (Dependency Injection)
    public MaterialsController(AppDbContext db) => _db = db;

    // GET: /api/materials – получить список всех материалов (доступно admin, manager, jeweler)
    [HttpGet]   // Атрибут указывает, что метод обрабатывает HTTP GET запросы
    public async Task<IActionResult> GetAll()
    {
        // Запрашиваем все материалы из таблицы Materials асинхронно
        var materials = await _db.Materials.ToListAsync();
        // Возвращаем HTTP 200 OK с JSON-массивом материалов
        return Ok(materials);
    }

    // POST: /api/materials – создать новый материал (только для администраторов)
    [Authorize(Roles = "admin")]// Переопределяем доступ – только роль admin
    [HttpPost]// Атрибут для обработки HTTP POST запросов
    public async Task<IActionResult> Create([FromBody] Material material)
    {
        // Проверяем, что название материала не пустое и не состоит из пробелов
        if (string.IsNullOrWhiteSpace(material.Name))
            return BadRequest("Название обязательно");// 400 Bad Request с сообщением

        // Добавляем новый материал в контекст (в состояние Added)
        _db.Materials.Add(material);
        // Сохраняем изменения в базе данных (вставка новой записи)
        await _db.SaveChangesAsync();
        // Возвращаем 200 OK с созданным материалом (включая сгенерированный MaterialId)
        return Ok(material);
    }

    // PUT: /api/materials/{id} – обновить существующий материал (только для администраторов)
    [Authorize(Roles = "admin")]// Только админ
    [HttpPut("{id}")] // {id} – параметр маршрута, указывает какой материал обновить
    public async Task<IActionResult> Update(int id, [FromBody] Material material)
    {
        // Ищем существующий материал по первичному ключу
        var existing = await _db.Materials.FindAsync(id);
        if (existing == null) return NotFound(); // Если не найден – 404

        // Обновляем все поля существующего материала значениями из переданного объекта
        existing.Name = material.Name;// Название материала
        existing.Unit = material.Unit;   // Единица измерения (г, кар, шт)
        existing.PricePerUnit = material.PricePerUnit;// Цена за единицу
        existing.QuantityInStock = material.QuantityInStock; // Количество на складе
        existing.Description = material.Description; // Описание

        // Сохраняем изменения в базе данных (UPDATE)
        await _db.SaveChangesAsync();
        // Возвращаем 200 OK с обновлённым объектом материала
        return Ok(existing);
    }

    // DELETE: /api/materials/{id} – удалить материал (только для администраторов)
    [Authorize(Roles = "admin")]   // Только админ
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Ищем материал по первичному ключу
        var material = await _db.Materials.FindAsync(id);
        if (material == null) return NotFound(); // Если не найден – 404

        // Удаляем материал из контекста (состояние Deleted)
        _db.Materials.Remove(material);
        // Сохраняем изменения в базе данных (DELETE)
        await _db.SaveChangesAsync();
        // Возвращаем 200 OK (без тела, или можно Ok(new { message = "Материал удалён" }))
        return Ok();
    }
}