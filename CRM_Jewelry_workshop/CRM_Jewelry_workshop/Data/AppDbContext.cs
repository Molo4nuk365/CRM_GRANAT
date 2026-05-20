using CRM_Jewelry_workshop.Models;
using SharpDX.Direct3D9;
using StackExchange.Redis;
using System.Data.Entity;
using System.Reflection.Emit;


namespace CRM_Jewelry_workshop.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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

        // Уникальный индекс на логин
        modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();

        // Внешние ключи для Order с каскадным поведением Restrict
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Client)
            .WithMany(u => u.OrdersAsClient)
            .HasForeignKey(o => o.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Manager)
            .WithMany(u => u.OrdersAsManager)
            .HasForeignKey(o => o.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Jeweler)
            .WithMany(u => u.OrdersAsJeweler)
            .HasForeignKey(o => o.JewelerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Admin)
            .WithMany(u => u.OrdersAsAdmin)
            .HasForeignKey(o => o.AdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
