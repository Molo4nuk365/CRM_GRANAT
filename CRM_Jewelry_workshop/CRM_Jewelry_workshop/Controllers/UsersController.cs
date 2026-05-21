// Подключаем атрибуты авторизации (Authorize) для ограничения доступа по ролям
using Microsoft.AspNetCore.Authorization;
// Подключаем функциональность MVC для создания API-контроллеров (ControllerBase, ApiController и др.)
using Microsoft.AspNetCore.Mvc;
// Подключаем Entity Framework Core для асинхронных запросов к БД (Include, AnyAsync и др.)
using Microsoft.EntityFrameworkCore;
// Подключаем наш контекст базы данных (AppDbContext) и модели данных
using CRM_Jewelry_workshop.Data;
// Подключаем DTO (Data Transfer Object) для передачи данных между клиентом и сервером
using CRM_Jewelry_workshop.DTO;

// Пространство имён для контроллеров API
namespace CRM_Jewelry_workshop.Controllers;

// Атрибут Authorize – доступ к этому контроллеру только для пользователей с ролью "admin"
[Authorize(Roles = "admin")]
// Атрибут ApiController – включает автоматическую валидацию модели, привязку [FromBody] и другие API-фичи
[ApiController]
// Базовый маршрут: все методы будут доступны по /api/users
[Route("api/[controller]")]
// Наследуемся от BaseController (вероятно, там определён CurrentUserId – ID текущего авторизованного пользователя)
public class UsersController : BaseController
{
    // Приватное поле для доступа к базе данных
    private readonly AppDbContext _db;

    // Конструктор – внедрение зависимости AppDbContext через DI (Dependency Injection)
    public UsersController(AppDbContext db) => _db = db;

    // GET: /api/users – получить список всех пользователей
    [HttpGet]   // Атрибут указывает, что метод обрабатывает HTTP GET запросы
  public async Task<IActionResult> GetAll()
    {
        // Запрос к таблице Users с подключением (Include) связанной сущности Role
   var users = await _db.Users
    .Include(u => u.Role)   // Подгружаем Role, чтобы получить RoleName
    .Select(u => new
    {   // Проецируем на анонимный тип – возвращаем только нужные поля
       u.UserId,           // ID пользователя
       u.Login,            // Логин
       u.FullName,         // Полное имя
       RoleName = u.Role!.RoleName // Название роли (оператор ! означает, что Role не null, т.к. мы сделали Include)
     })
     .ToListAsync(); // Выполняем запрос асинхронно и получаем список
        // Возвращаем HTTP 200 OK с телом в виде JSON (users)
        return Ok(users);
    }

    // PUT: /api/users/{id}/role – изменить роль пользователя
    [HttpPut("{id}/role")] // {id} – параметр маршрута
    public async Task<IActionResult> ChangeRole(int id, [FromBody] RoleChangeDto dto)
    {
        // Ищем пользователя по первичному ключу (UserId)
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(); // Если не найден – 404

        // Ищем роль по имени (dto.Role – например, "admin", "manager" и т.д.)
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
        if (role == null) return BadRequest("Роль не найдена"); // Если роль не существует – 400

        // Меняем внешний ключ RoleId у пользователя
        user.RoleId = role.RoleId;
        // Сохраняем изменения в базе данных
        await _db.SaveChangesAsync();
        // Возвращаем 200 OK без тела (или можно вернуть Ok(new { message = "Роль изменена" }))
        return Ok();
    }

    // DELETE: /api/users/{id} – удалить пользователя
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
     // Проверяем, не пытается ли пользователь удалить сам себя
     // CurrentUserId – свойство, унаследованное от BaseController (ID текущего авторизованного пользователя)
        if (id == CurrentUserId)
            return BadRequest(new { message = "Нельзя удалить себя" });

    // Ищем удаляемого пользователя
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

    // Проверяем, есть ли у пользователя заказы в любой из ролей (клиент, менеджер, ювелир, администратор)
        var hasOrders = await _db.Orders.AnyAsync(o =>
            o.ClientId == id || o.ManagerId == id || o.JewelerId == id || o.AdminId == id);
        if (hasOrders)
            return BadRequest(new { message = "Нельзя удалить пользователя, у которого есть заказы" });

    // Проверяем, не является ли пользователь администратором (админов удалять запрещено)
        var role = await _db.Roles.FindAsync(user.RoleId);
        if (role?.RoleName == "admin")
            return BadRequest(new { message = "Нельзя удалить администратора" });

    // Удаляем пользователя из контекста
        _db.Users.Remove(user);
    // Сохраняем изменения в БД
        await _db.SaveChangesAsync();
    // Возвращаем успешный ответ с сообщением
        return Ok(new { message = "Пользователь удалён" });
    }
}