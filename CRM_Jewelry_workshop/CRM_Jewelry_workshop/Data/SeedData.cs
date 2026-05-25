using BCrypt.Net;
using CRM_Jewelry_workshop.Data;
using CRM_Jewelry_workshop.Models;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Roles.Any()) return;

        // Роли
        var roles = new[]
        {
            new Role { RoleName = "admin", Description = "Полный доступ" },
            new Role { RoleName = "manager", Description = "Управление заказами" },
            new Role { RoleName = "jeweler", Description = "Выполнение заказов" },
            new Role { RoleName = "client", Description = "Покупка и заказы" }
        };
        db.Roles.AddRange(roles);
        db.SaveChanges();

        var adminRole = db.Roles.First(r => r.RoleName == "admin");
        var managerRole = db.Roles.First(r => r.RoleName == "manager");
        var jewelerRole = db.Roles.First(r => r.RoleName == "jeweler");
        var clientRole = db.Roles.First(r => r.RoleName == "client");

        var users = new[]
        {
            new User { Login = "admin",   PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),   RoleId = adminRole.RoleId, FullName = "Администратор", Email = "admin@example.com", Phone = "" },
            new User { Login = "manager", PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"), RoleId = managerRole.RoleId, FullName = "Менеджер", Email = "manager@example.com", Phone = "" },
            new User { Login = "jeweler", PasswordHash = BCrypt.Net.BCrypt.HashPassword("jeweler123"), RoleId = jewelerRole.RoleId, FullName = "Ювелир", Email = "jeweler@example.com", Phone = "" },
            new User { Login = "client",  PasswordHash = BCrypt.Net.BCrypt.HashPassword("client123"),  RoleId = clientRole.RoleId, FullName = "Клиент", Email = "client@example.com", Phone = "+7(999)111-22-33" }
        };
        db.Users.AddRange(users);
        db.SaveChanges();

        // Статусы заказов
        if (!db.StatusOrders.Any())
        {
            db.StatusOrders.AddRange(
                new StatusOrder { Name = "Новый" },
                new StatusOrder { Name = "Принят" },
                new StatusOrder { Name = "В работе" },
                new StatusOrder { Name = "Готов" },
                new StatusOrder { Name = "Завершён" },
                new StatusOrder { Name = "Отменён" }
            );
            db.SaveChanges();
        }

        // Статусы платежей
        if (!db.StatusPayments.Any())
        {
            db.StatusPayments.AddRange(
                new StatusPayment { Name = "pending" },
                new StatusPayment { Name = "paid" },
                new StatusPayment { Name = "refunded" }
            );
            db.SaveChanges();
        }

        // Товары
        var products = new[]
        {
            new Product { Name = "Кольцо «Гранатовый рассвет»", Type = "Кольцо", Metal = "Серебро 925", Stone = "Гранат 0.8 карат", Weight = 3.2m, Price = 18500, Description = "Изящное кольцо из серебра 925 пробы с натуральным гранатом. Огранка «маркиз».", Article = "GR-101", ImageUrl = "/images/кольцо.png" },
            new Product { Name = "Серьги «Лунный свет»", Type = "Серьги", Metal = "Серебро 925", Stone = "Гранат 0.5 карат (каждый)", Weight = 4.5m, Price = 12400, Description = "Элегантные серьги-гвоздики с гранатами в серебряной оправе.", Article = "GR-102", ImageUrl = "/images/серьги.png" },
            new Product { Name = "Подвеска «Капля росы»", Type = "Подвеска", Metal = "Серебро 925", Stone = "Гранат 1 карат", Weight = 1.8m, Price = 9800, Description = "Нежная подвеска в форме капли с крупным гранатом.", Article = "GR-103", ImageUrl = "/images/подвеска.jpg" },
            new Product { Name = "Браслет «Серебряная нить»", Type = "Браслет", Metal = "Серебро 925", Stone = "Гранат (вставки)", Weight = 6.2m, Price = 23500, Description = "Плетёный браслет с мелкими гранатами.", Article = "GR-104", ImageUrl = "/images/браслет.png" },
            new Product { Name = "Брошь «Гранат»", Type = "Брошь", Metal = "Серебро 925", Stone = "Гранат 2 карат", Weight = 5.1m, Price = 15900, Description = "Брошь в виде цветка граната.", Article = "GR-105", ImageUrl = "/images/брошь.jpg" }
        };
        db.Products.AddRange(products);
        db.SaveChanges();

        // Тестовые заказы (3 штуки)
        if (!db.Orders.Any())
        {
            var client = db.Users.First(u => u.Login == "client");
            var manager = db.Users.First(u => u.Login == "manager");
            var jeweler = db.Users.First(u => u.Login == "jeweler");
            var statusNew = db.StatusOrders.First(s => s.Name == "Новый");
            var product = db.Products.First();

            for (int i = 0; i < 3; i++)
            {
                db.Orders.Add(new Order
                {
                    ClientId = client.UserId,
                    ManagerId = manager.UserId,
                    JewelerId = jeweler.UserId,
                    StatusOrderId = statusNew.StatusOrderId,
                    CreateDate = DateTime.Now.AddDays(-i),
                    TotalCost = product.Price,
                    Deadline = null
                });
            }
            db.SaveChanges();
        }
    }
}