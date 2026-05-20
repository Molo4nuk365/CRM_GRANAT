using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Jewelry_workshop.Models
{
    public class User
    {
        
        [Key]
        public int UserId { get; set; }
        public int RoleId { get; set; }
        
        //Прописываем внешний ключ
        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }

        
        [Required]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // храним хеш, а не пароль

        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        // Навигационные свойства для заказов
        public ICollection<Order>? OrdersAsClient { get; set; }
        public ICollection<Order>? OrdersAsManager { get; set; }
        public ICollection<Order>? OrdersAsJeweler { get; set; }
        public ICollection<Order>? OrdersAsAdmin { get; set; }
    }
}
