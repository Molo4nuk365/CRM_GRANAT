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
    // Метод CreateDbContext вызывается инструментами EF Core (dotnet ef migrations add/update)
    public AppDbContext CreateDbContext(string[] args)
    {
        // Создаём билдер параметров контекста
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Настраиваем использование SQL Server с конкретной строкой подключения
        // Server – локальный экземпляр SQL Server Express 2022 года
        // Database – имя базы данных
        // Trusted_Connection=True – использовать Windows-аутентификацию
        // TrustServerCertificate=true – доверять самоподписанному сертификату (для разработки)
        optionsBuilder.UseSqlServer("Server=LAPTOP-QU8TEID8\\SQLEXPRESS02;Database=CRM_Jewelry_workshop;Trusted_Connection=True;TrustServerCertificate=true;"); 

        // Создаём и возвращаем новый экземпляр AppDbContext с переданными настройками
        return new AppDbContext(optionsBuilder.Options);
    }
}