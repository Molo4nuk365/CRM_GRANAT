using System.ComponentModel.DataAnnotations;      // Для атрибутов валидации (Key, Required)
using System.ComponentModel.DataAnnotations.Schema; // Для ForeignKey

namespace CRM_Jewelry_workshop.Models;

// Пользователь системы (админ, менеджер, ювелир, клиент)
public class User
{
    // Первичный ключ таблицы Users
    [Key] 
    // Атрибут указывает, что это свойство – уникальный идентификатор
    public int UserId { get; set; }

    // Внешний ключ на таблицу Roles
    public int RoleId { get; set; }

    // Навигационное свойство для связи с таблицей Roles
    [ForeignKey(nameof(RoleId))] 
    // Указываем, что RoleId – внешний ключ
    public Role? Role { get; set; } 
    // Nullable, потому что может быть не загружен (но в БД RoleId обязателен)

    // Логин пользователя (уникальный, обязательный)
    [Required]
    public string Login { get; set; } = string.Empty;

    // Хеш пароля (BCrypt), а не сам пароль – для безопасности
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    // Контактные данные
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    // Навигационные свойства для заказов
    // (только один пользователь может быть клиентом, менеджером, ювелиром или админом)
    public ICollection<Order>? OrdersAsClient { get; set; }   
    // Заказы как клиент
    public ICollection<Order>? OrdersAsManager { get; set; }  
    // Заказы как менеджер
    public ICollection<Order>? OrdersAsJeweler { get; set; }  
    // Заказы как ювелир
    public ICollection<Order>? OrdersAsAdmin { get; set; }    
    // Заказы как администратор
}