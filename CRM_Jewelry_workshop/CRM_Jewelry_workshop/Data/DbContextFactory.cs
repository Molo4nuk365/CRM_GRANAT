using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRM_Jewelry_workshop.Data;

public class DbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        optionsBuilder.UseSqlServer ("Server=LAPTOP-QU8TEID8\\SQLEXPRESS02;Database=CRM_Jewelry_workshop;Trusted_Connection=True;TrustServerCertificate=true;");
        return new AppDbContext(optionsBuilder.Options);
    }
}
