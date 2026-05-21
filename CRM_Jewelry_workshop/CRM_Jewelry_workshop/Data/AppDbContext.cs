// Подключаем Entity Framework Core (для DbContext, DbSet, ModelBuilder и методов работы с БД)
using Microsoft.EntityFrameworkCore;
// Подключаем модели данных (User, Role, Order, Product и т.д.)
using CRM_Jewelry_workshop.Models;
// Пространство имён, где хранится контекст базы данных
namespace CRM_Jewelry_workshop.Data;
// Класс AppDbContext – главный класс для взаимодействия с базой данных
public class AppDbContext : DbContext
{
    // Конструктор, принимающий параметры подключения (передаются через
    // DI — это способ передачи зависимостей (объектов, от которых зависит другой объект)
    //извне внутрь класса или метода в Program.cs)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Таблицы базы данных (каждое свойство DbSet<T> соответствует таблице в БД)
    // Используем выражение-член (=>) для краткой записи get-only свойства
    public DbSet<User> Users => Set<User>();  // Таблица пользователей
    public DbSet<Role> Roles => Set<Role>(); // Таблица ролей
    public DbSet<Product> Products => Set<Product>();// Таблица товаров (изделий)
    public DbSet<Material> Materials => Set<Material>();// Таблица материалов
    public DbSet<Order> Orders => Set<Order>();// Таблица заказов
    public DbSet<Position> Positions => Set<Position>();// Таблица позиций (связь заказ-материал)
    public DbSet<Payment> Payments => Set<Payment>();// Таблица платежей
    public DbSet<StatusOrder> StatusOrders => Set<StatusOrder>();// Таблица статусов заказов
    public DbSet<StatusPayment> StatusPayments => Set<StatusPayment>(); // Таблица статусов платежей

    // Переопределяем метод OnModelCreating – здесь задаются дополнительные настройки модели
    // (индексы, ограничения, связи между таблицами, поведение при удалении)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Вызываем базовую реализацию (стандартные настройки EF Core)
        base.OnModelCreating(modelBuilder);

        // Создаём уникальный индекс на поле Login в таблице Users
        // Это гарантирует, что не будет двух пользователей с одинаковым логином
        modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();

        // Настройка внешних ключей для таблицы Orders (заказы)
        // Для каждой связи запрещаем каскадное удаление (Restrict),
        // чтобы при удалении пользователя не удалялись связанные с ним заказы

        // Связь: Order (заказ) - Client (клиент) – один клиент может иметь много заказов
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Client)// У заказа есть один клиент
            .WithMany(u => u.OrdersAsClient)// У пользователя может быть много заказов как клиента
            .HasForeignKey(o => o.ClientId)  // Внешний ключ – ClientId
            .OnDelete(DeleteBehavior.Restrict);// Запрещаем каскадное удаление

        // Связь: Order → Manager (менеджер) – один менеджер может вести много заказов
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Manager)
            .WithMany(u => u.OrdersAsManager)
            .HasForeignKey(o => o.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Связь: Order → Jeweler (ювелир) – один ювелир может выполнять много заказов
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Jeweler)
            .WithMany(u => u.OrdersAsJeweler)
            .HasForeignKey(o => o.JewelerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Связь: Order → Admin (администратор) – один администратор может быть связан с многими заказами
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Admin)
            .WithMany(u => u.OrdersAsAdmin)
            .HasForeignKey(o => o.AdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}