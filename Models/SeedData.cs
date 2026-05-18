using BCrypt.Net;
using CRM_Granat.Data;
namespace CRM_Granat.Models
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext db)
        {
            db.Database.EnsureCreated();
            if (db.Users.Any()) return;

            var users = new User[]
            {
            new User { Login = "admin", PasswordHash = BCrypt.HashPassword("admin123"), Role = "admin", FullName = "Администратор", Phone = "", Address = "" },
            new User { Login = "manager", PasswordHash = BCrypt.HashPassword("manager123"), Role = "manager", FullName = "Анна Иванова", Phone = "", Address = "" },
            new User { Login = "jeweler", PasswordHash = BCrypt.HashPassword("jeweler123"), Role = "jeweler", FullName = "Сергей Петров", Phone = "", Address = "" },
            new User { Login = "client", PasswordHash = BCrypt.HashPassword("client123"), Role = "client", FullName = "Клиент Тестов", Phone = "+7 (999) 123-4567", Address = "Москва" }
            };
            db.Users.AddRange(users);
            db.SaveChanges();

            var products = new Product[]
            {
            new Product { Name = "Кольцо «Гранатовый рассвет»", Price = 18500, Description = "Серебро 925, гранат 0.8 карат", Material = "Серебро 925, гранат", Weight = "3.2 г", Article = "GR-101", ImageUrl = "images/кольцо.png" },
            new Product { Name = "Серьги «Лунный свет»", Price = 12400, Description = "Серебро 925, гранат 0.5 карат", Material = "Серебро 925, гранат", Weight = "4.5 г", Article = "GR-102", ImageUrl = "images/серьги.png" },
            new Product { Name = "Подвеска «Капля росы»", Price = 9800, Description = "Серебро 925, гранат 1 карат", Material = "Серебро 925, гранат", Weight = "1.8 г", Article = "GR-103", ImageUrl = "images/подвеска.jpg" },
            new Product { Name = "Браслет «Золотая нить»", Price = 23500, Description = "Серебро 925, гранат", Material = "Серебро 925, гранат", Weight = "6.2 г", Article = "GR-104", ImageUrl = "images/браслет.jpg" },
            new Product { Name = "Брошь «Гранат»", Price = 15900, Description = "Серебро 925, гранат 2 карат", Material = "Серебро 925, гранат", Weight = "5.1 г", Article = "GR-105", ImageUrl = "images/брошь.jpg" }
            };
            db.Products.AddRange(products);
            db.SaveChanges();

            var materials = new Material[]
            {
            new Material { Name = "Серебро 925", Quantity = 980, Unit = "г", PricePerUnit = 80 },
            new Material { Name = "Гранат", Quantity = 150, Unit = "кар", PricePerUnit = 1200 }
            };
            db.Materials.AddRange(materials);
            db.SaveChanges();

            var repairs = new RepairOption[]
            {
            new RepairOption { Name = "Ремонт кольца", Price = 3500, Description = "Пайка, полировка, изменение размера" },
            new RepairOption { Name = "Ремонт серёг", Price = 2800, Description = "Замена замка, полировка" },
            new RepairOption { Name = "Ремонт цепочки", Price = 2200, Description = "Пайка звеньев, замена замка" },
            new RepairOption { Name = "Чистка и полировка", Price = 1200, Description = "Ультразвуковая чистка, полировка" }
            };
            db.RepairOptions.AddRange(repairs);
            db.SaveChanges();

            var orders = new Order[]
            {
            new Order { ClientId = 4, Type = "product", ProductId = 1, Status = "completed", ManagerId = 2, JewelerId = 3, Deadline = DateTime.Parse("2026-05-20"), Total = 18500, CompletedDate = DateTime.Parse("2026-05-18"), PaymentStatus = "paid" },
            new Order { ClientId = 4, Type = "product", ProductId = 2, Status = "completed", ManagerId = 2, JewelerId = 3, Deadline = DateTime.Parse("2026-05-25"), Total = 12400, CompletedDate = DateTime.Parse("2026-05-22"), PaymentStatus = "paid" },
            new Order { ClientId = 4, Type = "repair", RepairId = 101, Status = "in_progress", ManagerId = 2, JewelerId = 3, Deadline = DateTime.Parse("2026-06-01"), Total = 3500, PaymentStatus = "pending" }
            };
            db.Orders.AddRange(orders);
            db.SaveChanges();
        }
    }
}
