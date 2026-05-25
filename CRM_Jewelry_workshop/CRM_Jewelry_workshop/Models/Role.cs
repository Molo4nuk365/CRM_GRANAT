// Подключаем пространство имён для атрибутов валидации (Key, Required и др.)
using System.ComponentModel.DataAnnotations;

// Объявляем пространство имён, где хранятся модели приложения
namespace CRM_Jewelry_workshop.Models
{
    // Роль пользователя (определяет права доступа)
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        // Название роли: "admin", "manager", "jeweler", "client"
        public string RoleName { get; set; } = string.Empty;

        // Описание роли (для информативности)
        public string Description { get; set; } = string.Empty;

        // Один ко многим: одна роль может быть у многих пользователей
        public ICollection<User>? Users { get; set; }
    }
}