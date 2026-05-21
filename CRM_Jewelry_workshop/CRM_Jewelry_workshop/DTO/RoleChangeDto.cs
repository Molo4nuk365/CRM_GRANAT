// Пространство имён для DTO (Data Transfer Objects) – объекты для передачи данных между клиентом и сервером
namespace CRM_Jewelry_workshop.DTO
{
 // DTO для смены роли пользователя (используется в UsersController.ChangeRole)
    public class RoleChangeDto
    {
        // Свойство Role – название новой роли (например, "admin", "manager", "jeweler", "client")
        // Будет передаваться в теле запроса (JSON)
        public string Role { get; set; }
    }
}