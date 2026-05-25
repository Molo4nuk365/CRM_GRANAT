using Microsoft.EntityFrameworkCore;
using CRM_Jewelry_workshop.Models;

namespace CRM_Jewelry_workshop.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Путь к таблицам, которые лежат в папке Models
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StatusOrder> StatusOrders => Set<StatusOrder>();
    public DbSet<StatusPayment> StatusPayments => Set<StatusPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Уникальный индекс на логин пользователя
        modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();

        // Настройка связей для Order (для того чтобы EF Core понимал, какое навигационное свойство User использовать)
        // Order - Client
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Client)
            .WithMany(u => u.OrdersAsClient)
            .HasForeignKey(o => o.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order - Manager
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Manager)
            .WithMany(u => u.OrdersAsManager)
            .HasForeignKey(o => o.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order - Jeweler
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Jeweler)
            .WithMany(u => u.OrdersAsJeweler)
            .HasForeignKey(o => o.JewelerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order - Admin 
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Admin)
            .WithMany(u => u.OrdersAsAdmin)
            .HasForeignKey(o => o.AdminId)
            .OnDelete(DeleteBehavior.Restrict);

        // В SQLite отключаем каскадное удаление для всех внешних ключей (чтобы избежать ошибок)
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}