using Microsoft.EntityFrameworkCore;
using CRM_Granat.Models;
using CRM_Granat.DTOs;

namespace CRM_Granat.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<RepairOption> RepairOptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();
    }
}
