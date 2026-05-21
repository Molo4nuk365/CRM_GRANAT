// Подключаем библиотеку BCrypt для безопасного хеширования паролей
using BCrypt.Net;
// Подключаем модели данных (User, Role, Order, Product и др.)
using CRM_Jewelry_workshop.Models;

// Пространство имён для данных (инициализация БД)
namespace CRM_Jewelry_workshop.Data;

// Статический класс SeedData – содержит метод для заполнения базы начальными данными
public static class SeedData
{
    // Метод Initialize – добавляет записи в БД, если они отсутствуют
    public static void Initialize(AppDbContext db)
    {
        // Если в таблице Roles уже есть какие-либо записи – выходим (данные уже добавлены)
        if (db.Roles.Any()) return;

        // Создание ролей
        var roles = new[]
        {
            new Role { RoleName = "admin", Description = "Полный доступ" },
            new Role { RoleName = "manager", Description = "Управление заказами" },
            new Role { RoleName = "jeweler", Description = "Выполнение заказов" },
            new Role { RoleName = "client", Description = "Покупка и заказы" }
        };
        // Добавляем массив ролей в контекст
        db.Roles.AddRange(roles);
        // Сохраняем изменения в БД (теперь роли имеют сгенерированные RoleId)
        db.SaveChanges();

        // Создание пользователей (пароли хешируются BCrypt)
        // Получаем созданные роли из БД, чтобы привязать к пользователям
        var adminRole = db.Roles.First(r => r.RoleName == "admin");
        var managerRole = db.Roles.First(r => r.RoleName == "manager");
        var jewelerRole = db.Roles.First(r => r.RoleName == "jeweler");
        var clientRole = db.Roles.First(r => r.RoleName == "client");

        // Массив пользователей с предопределёнными логинами, хешированными паролями и ролями
        var users = new[]
        {
            new User { Login = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), RoleId = adminRole.RoleId, FullName = "Администратор", Email = "admin@example.com", Phone = "" },
            new User { Login = "manager", PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"), RoleId = managerRole.RoleId, FullName = "Менеджер", Email = "manager@example.com", Phone = "" },
            new User { Login = "jeweler", PasswordHash = BCrypt.Net.BCrypt.HashPassword("jeweler123"), RoleId = jewelerRole.RoleId, FullName = "Ювелир", Email = "jeweler@example.com", Phone = "" },
            new User { Login = "client", PasswordHash = BCrypt.Net.BCrypt.HashPassword("client123"), RoleId = clientRole.RoleId, FullName = "Клиент", Email = "client@example.com", Phone = "+7(999)111-22-33" }
        };
        db.Users.AddRange(users);
        db.SaveChanges();

        //Статусы заказов (добавляем только если таблица пуста)
        if (!db.StatusOrders.Any())
        {
            db.StatusOrders.AddRange(
                new StatusOrder { Name = "new" },
                new StatusOrder { Name = "in_progress" },
                new StatusOrder { Name = "completed" },
                new StatusOrder { Name = "cancelled" }
            );
            db.SaveChanges();
        }

        //Статусы платежей (добавляем только если таблица пуста)
        if (!db.StatusPayments.Any())
        {
            db.StatusPayments.AddRange(
                new StatusPayment { Name = "pending" },
                new StatusPayment { Name = "paid" },
                new StatusPayment { Name = "refunded" }
            );
            db.SaveChanges();
        }

        // Товары (изделия) – каталог продукции
        var products = new[]
        {
            new Product { Name = "Кольцо «Гранатовый рассвет»", Price = 18500, Description = "Серебро 925, гранат 0.8 карат", Article = "GR-101", ImageUrl = "/images/кольцо.png" },
            new Product { Name = "Серьги «Лунный свет»", Price = 12400, Description = "Серебро 925, гранат 0.5 карат", Article = "GR-102", ImageUrl = "/images/серьги.png" },
            new Product { Name = "Подвеска «Капля росы»", Price = 9800, Description = "Серебро 925, гранат 1 карат", Article = "GR-103", ImageUrl = "/images/подвеска.jpg" },
            new Product { Name = "Браслет «Серебряная нить»", Price = 23500, Description = "Серебро 925", Article = "GR-104", ImageUrl = "/images/браслет.png" },
            new Product { Name = "Брошь «Гранат»", Price = 15900, Description = "Серебро 925, гранат 2 карат", Article = "GR-105", ImageUrl = "/images/брошь.jpg" }
        };
        db.Products.AddRange(products);
        db.SaveChanges();

        //Тестовые заказы (3 штуки), чтобы сразу увидеть данные в интерфейсе
        if (!db.Orders.Any())
        {
            // Находим нужных пользователей и статус
            var client = db.Users.First(u => u.Login == "client");
            var manager = db.Users.First(u => u.Login == "manager");
            var jeweler = db.Users.First(u => u.Login == "jeweler");
            var statusNew = db.StatusOrders.First(s => s.Name == "new");
            var product = db.Products.First(); // берём первый товар для суммы

            // Создаём 3 одинаковых заказа (для демонстрации)
            for (int i = 0; i < 3; i++)
            {
                db.Orders.Add(new Order
                {
                    ClientId = client.UserId,
                    ManagerId = manager.UserId,
                    JewelerId = jeweler.UserId,
                    StatusOrderId = statusNew.StatusOrderId,
                    CreateDate = DateTime.Now,
                    TotalCost = product.Price,
                    Deadline = null
                });
            }
            db.SaveChanges();
        }
    }
}