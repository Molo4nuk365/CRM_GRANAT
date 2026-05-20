using System.ComponentModel.DataAnnotations;

namespace CRM_Jewelry_workshop.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty; 
        // admin, manager, jeweler, client, accountant
        public string Description { get; set; } = string.Empty;
        public ICollection<User>? Users { get; set; }
    }
}
