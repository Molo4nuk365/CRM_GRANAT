using Microsoft.AspNetCore.Authorization; 
// Атрибуты [Authorize] для ограничения доступа к контроллеру/методам на основе ролей
using Microsoft.AspNetCore.Mvc;           
// Базовые классы и атрибуты для Web API (ApiController, Route, HttpGet и др.)
using Microsoft.EntityFrameworkCore;     
// Методы расширения EF Core (ToListAsync, FindAsync) для асинхронных запросов
using CRM_Jewelry_workshop.Data;          
// Контекст базы данных AppDbContext — точка входа ко всем таблицам
using CRM_Jewelry_workshop.Models;        
// Модели сущностей (Material, Product, Order и т.д.)

namespace CRM_Jewelry_workshop.Controllers;

// Контроллер для управления справочником материалов.
// На уровне класса задано [Authorize(Roles = "admin,manager,jeweler")], поэтому все методы,
// если не переопределено, доступны только этим трём ролям.
[Authorize(Roles = "admin,manager,jeweler")]
[ApiController]                      
// Автоматическая проверка модели и привязка параметров
[Route("api/[controller]")]          
// Маршрут: api/Materials
public class MaterialsController : BaseController 
// Наследуем BaseController, получая CurrentUserId, CurrentUserRole и т.д.
{
    private readonly AppDbContext _db; 
    
// Контекст БД, внедряемый через конструктор

    // Конструктор с DI
    public MaterialsController(AppDbContext db) => _db = db;

    // GET api/materials — получить список всех материалов.
    // Доступен трём ролям согласно атрибуту класса.
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
    // Возвращаем 200 OK с массивом материалов, загруженных из БД асинхронно
        Ok(await _db.Materials.ToListAsync());

    // POST api/materials — создать новый материал.
    // Переопределяем ограничение: доступно только администратору.
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Material material)
    {
        // Простейшая валидация: название материала не должно быть пустым или состоять из пробелов
        if (string.IsNullOrWhiteSpace(material.Name)) return BadRequest("Название обязательно");

        // Добавляем материал в контекст (пока в памяти)
        _db.Materials.Add(material);
        // Сохраняем изменения в БД, после чего material получит MaterialId
        await _db.SaveChangesAsync();
        // Возвращаем созданный материал
        return Ok(material);
    }

    // PUT api/materials/{id} — обновить существующий материал.
    // Только администратор.
    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Material material)
    {
        // Находим существующий материал по ID
        var existing = await _db.Materials.FindAsync(id);
        // Если не найден — 404
        if (existing == null) return NotFound();

        // Поштучно обновляем все редактируемые поля
        existing.Name = material.Name;
        existing.Unit = material.Unit;
        existing.PricePerUnit = material.PricePerUnit;
        existing.QuantityInStock = material.QuantityInStock;
        existing.Description = material.Description;

        // Сохраняем изменения в БД
        await _db.SaveChangesAsync();
        // Возвращаем обновлённый объект
        return Ok(existing);
    }

    // DELETE api/materials/{id} — удалить материал.
    // Только администратор.
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Ищем материал по первичному ключу
        var material = await _db.Materials.FindAsync(id);
        // Если не существует — возвращаем 404
        if (material == null) return NotFound();

        // Удаляем объект из контекста
        _db.Materials.Remove(material);
        // Применяем удаление в базе данных
        await _db.SaveChangesAsync();

        // Возвращаем 200 OK без содержимого
        // (можно было бы вернуть NoContent() для 204)
        return Ok();
    }
}