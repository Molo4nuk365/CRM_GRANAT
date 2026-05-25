using CRM_Jewelry_workshop.Data;        
// Контекст базы данных — точка входа для запросов к таблицам
using CRM_Jewelry_workshop.DTO;          
// DTO-классы, используемые в действиях контроллера (например, RoleChangeDto)
using Microsoft.AspNetCore.Authorization; 
// Атрибуты авторизации ([Authorize]) для ограничения доступа по ролям
using Microsoft.AspNetCore.Mvc;           
// Базовые классы и атрибуты для создания Web API контроллеров
using Microsoft.EntityFrameworkCore;     
// Методы Include, ToListAsync и другие расширения EF Core

namespace CRM_Jewelry_workshop.Controllers
{
    // Атрибут [Authorize] на уровне контроллера означает, что все методы контроллера доступны
    // только аутентифицированным пользователям с ролью "admin" или "manager".
    // Это базовая защита, которая может быть переопределена на уровне конкретного метода.
    [Authorize(Roles = "admin,manager")]
    [ApiController]                      
    // Включает автоматическую валидацию модели и привязку параметров
    [Route("api/[controller]")]          
    // Определяет базовый маршрут: api/Users
    public class UsersController : BaseController 
    // Наследуемся от BaseController, где может быть свойство CurrentUserId
    {
        private readonly AppDbContext _db; 
        // Поле для доступа к БД, внедряется через конструктор

        // Внедрение контекста базы данных через конструктор
        public UsersController(AppDbContext db) => _db = db;

        // GET api/users — получение списка всех пользователей с их ролями.
        // Доступ ограничен уровнем контроллера (admin, manager), поэтому отдельный [Authorize] не требуется.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Запрашиваем всех пользователей из базы, подгружая связанную сущность Role,
            // чтобы отобразить название роли вместо RoleId.
            var users = await _db.Users
                .Include(u => u.Role)   
                // Жадная загрузка связи User -> Role (чтобы не делать доп. запрос)
                .Select(u => new        
                // Проецируем только нужные поля в анонимный объект
                {
                    u.UserId,
                    u.Login,
                    u.FullName,
                    RoleName = u.Role!.RoleName  
                // Используем "!" чтобы сообщить компилятору, что Role не null (загружена)
                })
                .ToListAsync();

            // Возвращаем HTTP 200 OK с полученным списком
            return Ok(users);
        }

        // PUT api/users/{id}/role — изменение роли пользователя.
        // Атрибут [Authorize(Roles = "admin")] переопределяет уровень контроллера,
        // ограничивая доступ только администраторам.
        [Authorize(Roles = "admin")]
        [HttpPut("{id}/role")]
        public async Task<IActionResult> ChangeRole(int id, [FromBody] RoleChangeDto dto)
        {
            // Ищем пользователя по переданному идентификатору
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound(); 
            // Если не найден — возвращаем 404 Not Found

            // Ищем роль по её названию, переданному в DTO (например, "client", "jeweler")
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
            if (role == null) return BadRequest("Роль не найдена"); 
            // Роль не существует — возвращаем 400 Bad Request

            // Присваиваем пользователю найденную роль
            user.RoleId = role.RoleId;
            await _db.SaveChangesAsync(); 
            // Сохраняем изменения в БД

            // Возвращаем успешный ответ без содержимого (200 OK)
            return Ok();
        }

        // DELETE api/users/{id} — удаление пользователя.
        // Доступно только администратору (переопределяет контроллерный [Authorize]).
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Проверка, что администратор не пытается удалить сам себя.
            // CurrentUserId, скорее всего, определён в BaseController и получает Id текущего пользователя из токена.
            if (id == CurrentUserId)
                return BadRequest(new { message = "Нельзя удалить себя" });

            // Ищем пользователя по Id
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound(); 
            // Пользователь не найден — 404

            // Проверяем, есть ли у пользователя связанные заказы в любом из полей:
            // ClientId, ManagerId, JewelerId или AdminId.
            // AnyAsync возвращает true, если есть хотя бы один подходящий заказ.
            var hasOrders = await _db.Orders.AnyAsync(o =>
                o.ClientId == id || o.ManagerId == id || o.JewelerId == id || o.AdminId == id);
            if (hasOrders)
            // Если есть заказы, удалять нельзя — возвращаем сообщение об ошибке
                return BadRequest(new { message = "Нельзя удалить пользователя с заказами" });

            // Дополнительно проверяем, не является ли удаляемый пользователь администратором.
            // Это предотвращает удаление других администраторов.
            var role = await _db.Roles.FindAsync(user.RoleId);
            if (role?.RoleName == "admin")
                return BadRequest(new { message = "Нельзя удалить администратора" });

            // Удаляем пользователя из контекста
            _db.Users.Remove(user);
            await _db.SaveChangesAsync(); // Применяем удаление в БД

            // Возвращаем сообщение об успешном удалении
            return Ok(new { message = "Пользователь удалён" });
        }
    }
}