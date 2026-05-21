// Пространство имён для DTO (Data Transfer Objects) – объекты для передачи данных между клиентом и сервером
namespace CRM_Jewelry_workshop.DTOs;

// DTO для регистрации нового пользователя (используется в AuthController.Register)
public class RegisterDto
{
    // Логин пользователя (уникальный идентификатор для входа)
    public string Login { get; set; } = string.Empty;

    // Пароль в открытом виде (на сервере будет хеширован BCrypt)
    public string Password { get; set; } = string.Empty;

    // Полное имя (ФИО) пользователя
    public string FullName { get; set; } = string.Empty;

    // Адрес электронной почты
    public string Email { get; set; } = string.Empty;

    // Номер телефона (для связи)
    public string Phone { get; set; } = string.Empty;
}