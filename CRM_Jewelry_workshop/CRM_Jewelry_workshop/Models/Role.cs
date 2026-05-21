// Подключаем пространство имён для атрибутов валидации (Key, Required и др.)
using System.ComponentModel.DataAnnotations;

// Объявляем пространство имён, где хранятся модели приложения
namespace CRM_Jewelry_workshop.Models
{
    // Класс Role представляет роль пользователя в системе (admin, manager, jeweler, client, accountant)
    public class Role
    {
        // Первичный ключ таблицы Roles в базе данных
       
        [Key] // Атрибут указывает, что это свойство – уникальный идентификатор
        public int RoleId { get; set; }

        // Название роли (admin, manager, jeweler, client, accountant) – используется для проверки прав доступа
        public string RoleName { get; set; } = string.Empty;   // Инициализируем пустой строкой (non-nullable)

        // Описание роли (например, "Полный доступ", "Управление заказами" и т.д.)
        public string Description { get; set; } = string.Empty;

        // Навигационное свойство: одна роль может быть у многих пользователей
        // ICollection<User> – коллекция объектов User, принадлежащих этой роли
        // Знак ? означает, что коллекция может быть не загружена (зависит от Include в запросе)
        public ICollection<User>? Users { get; set; }
    }
}