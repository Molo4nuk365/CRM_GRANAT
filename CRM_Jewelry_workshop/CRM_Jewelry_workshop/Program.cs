// Подключаем модуль аутентификации через JWT (JSON Web Tokens)
using Microsoft.AspNetCore.Authentication.JwtBearer;
// Entity Framework Core для работы с БД
using Microsoft.EntityFrameworkCore;
// Валидация JWT токенов (ключи, издатель, аудитория)
using Microsoft.IdentityModel.Tokens;
// Для преобразования строки в байтовый массив (ключ шифрования)
using System.Text;
// Для игнорирования циклических ссылок при сериализации JSON
using System.Text.Json.Serialization;
// Наши собственные классы: контекст БД и инициализатор данных
using CRM_Jewelry_workshop.Data;
using CRM_Jewelry_workshop.Services;


// Создаём строитель веб-приложения
var builder = WebApplication.CreateBuilder(args);

// Создаём папку Data, если её нет (для SQLite-файла)
var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

// Регистрируем контроллеры и Swagger (отслеживает бэкенд)
builder.Services.AddControllers()
    // Настройка JSON: игнорируем циклические ссылки, чтобы избежать ошибок сериализации
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Добавляет поддержку API-контроллеров
builder.Services.AddEndpointsApiExplorer();
// Необходимо для Swagger
builder.Services.AddSwaggerGen();

// Регистрируем (берем) контекст базы данных с SQLite
// Берём строку подключения из appsettings.json по ключу "CRM_Jewelry_workshop"
var connectionString = builder.Configuration.GetConnectionString("CRM_Jewelry_workshop");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));       // Используем SQLite как СУБД

// Регистрируем наш сервис для работы с заказами
// AddScoped – создаёт один экземпляр на HTTP-запрос
builder.Services.AddScoped<IOrderService, OrderService>();

// Настройка JWT аутентификации
// Читаем ключ из конфигурации и преобразуем в байтовый массив
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Параметры проверки токена
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,      // Проверять ключ подписи
            IssuerSigningKey = new SymmetricSecurityKey(key), // Тот же ключ, что при создании
            ValidateIssuer = true,                // Проверять издателя (iss)
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,              // Проверять получателя (aud)
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true               // Проверять срок действия
        };
    });

// CORS – разрешаем запросы с любых источников (для удобства разработки)
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Завершаем настройку сервисов и строим приложение
var app = builder.Build();

// Инициализация базы данных (создание таблиц и начальных данных)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // EnsureCreated создаст файл .db и таблицы, если их нет
    db.Database.EnsureCreated();
    // SeedData заполняет справочники (роли, статусы) и тестовые данные
    SeedData.Initialize(db);
}

// Конвейер обработки HTTP-запросов (Middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();      // Генерирует JSON-спецификацию API
    app.UseSwaggerUI();    // Показывает интерактивный UI для тестирования
}

app.UseHttpsRedirection();   // Перенаправляет HTTP на HTTPS
app.UseCors("AllowAll");     // Подключаем CORS
app.UseAuthentication();     // Аутентификация – проверяет JWT
app.UseAuthorization();      // Авторизация – проверяет роли
app.MapControllers();        // Сопоставляет маршруты с методами контроллеров

// Для статических файлов фронтенда (HTML, CSS, JS)
app.UseDefaultFiles();       // По умолчанию ищет index.html в wwwroot
app.UseStaticFiles();        // Обслуживает файлы из папки wwwroot

// Запускаем проект
app.Run();