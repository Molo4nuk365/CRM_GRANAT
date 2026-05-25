// Подключаем Entity Framework Core для работы с БД (DbContext, DbContextOptionsBuilder)
using Microsoft.EntityFrameworkCore;
// Подключаем дизайн-время фабрику для создания контекста при миграциях
using Microsoft.EntityFrameworkCore.Design;

// Пространство имён для данных (фабрика контекста)
namespace CRM_Jewelry_workshop.Data;

// Реализуем интерфейс IDesignTimeDbContextFactory<AppDbContext>
// Это позволяет EF Core создавать экземпляр AppDbContext во время разработки (для миграций)
public class DbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // Строка подключения – такая же, как в appsettings.json
        optionsBuilder.UseSqlite("Data Source=.\\Data\\SQlLiteDatabase.db");
        return new AppDbContext(optionsBuilder.Options);
    }
}
    
