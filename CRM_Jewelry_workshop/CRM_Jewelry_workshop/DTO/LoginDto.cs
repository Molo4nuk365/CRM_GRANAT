// Пространство имён для DTO (Data Transfer Objects) – объекты для передачи данных между клиентом и сервером
namespace CRM_Jewelry_workshop.DTOs;

// DTO для входа пользователя (логин + пароль). Используется в AuthController.Login
public class LoginDto
{
// Логин пользователя – передаётся в теле POST-запроса на /api/auth/login
    public string Login { get; set; } = string.Empty;

// Пароль в открытом виде (сервер будет проверять через BCrypt.Verify)
    public string Password { get; set; } = string.Empty;
}