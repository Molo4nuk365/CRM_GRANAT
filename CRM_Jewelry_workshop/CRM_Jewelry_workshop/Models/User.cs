using System.ComponentModel.DataAnnotations;      // Подключаем пространство имён для атрибутов валидации (Required, Key и др.)
using System.ComponentModel.DataAnnotations.Schema; // Подключаем для атрибута ForeignKey (указание внешнего ключа)

namespace CRM_Jewelry_workshop.Models  // Пространство имён, где хранятся модели (сущности) приложения
{
 // Класс User представляет пользователя системы (админ, менеджер, ювелир, клиент)
    public class User
    {
 // Уникальный идентификатор пользователя (первичный ключ в таблице Users)
        [Key]  // Атрибут указывает, что это свойство – первичный ключ
        public int UserId { get; set; }

 // Внешний ключ – идентификатор роли, к которой принадлежит пользователь
        public int RoleId { get; set; }

 // Навигационное свойство для связи с таблицей Roles (один пользователь – одна роль)
 // Атрибут ForeignKey указывает, что свойство RoleId является внешним ключом для этой связи
        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; } // Nullable, потому что роль может быть не загружена (но в БД RoleId обязателен)

  // Логин пользователя (используется при входе) – обязательное поле
        [Required] // Атрибут – поле не может быть null или пустым
        public string Login { get; set; } = string.Empty; // Инициализируем пустой строкой, чтобы избежать null

 // Хеш пароля (не сам пароль, а его хеш – для безопасности)
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

// Электронная почта 
        public string Email { get; set; } = string.Empty;

// Телефон 
        public string Phone { get; set; } = string.Empty;

// Полное имя пользователя (ФИО)
        public string FullName { get; set; } = string.Empty;
// Навигационные свойства для заказов 
// Один пользователь может быть клиентом во многих заказах
        public ICollection<Order>? OrdersAsClient { get; set; }

        // Один пользователь может быть менеджером во многих заказах
        public ICollection<Order>? OrdersAsManager { get; set; }

        // Один пользователь может быть ювелиром во многих заказах
        public ICollection<Order>? OrdersAsJeweler { get; set; }

        // Один пользователь может быть администратором во многих заказах
        public ICollection<Order>? OrdersAsAdmin { get; set; }
    }
}