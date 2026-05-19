using CRM_Granat.Models;

namespace CRM_Granat.Controllers
{
    internal class Users : User
    {
        public string Login { get; set; }
        public object PasswordHash { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}