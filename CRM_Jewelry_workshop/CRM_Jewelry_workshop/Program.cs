// Подключаем модуль аутентификации через JWT (JSON Web Tokens)
using Microsoft.AspNetCore.Authentication.JwtBearer;
// Entity Framework Core для работы с БД
using Microsoft.EntityFrameworkCore;
// Валидация JWT токенов (ключи, издатель, аудитория)
using Microsoft.IdentityModel.Tokens;
// Для преобразования строки в байтовый массив (ключ шифрования)
using System.Text;
// Наши собственные классы: контекст БД и инициализатор данных
using CRM_Jewelry_workshop.Data;

// Создаём построитель веб-приложения (считывает конфигурацию, добавляет сервисы)
var builder = WebApplication.CreateBuilder(args);

// Регистрируем контроллеры (API-эндпоинты) – для обработки HTTP-запросов
builder.Services.AddControllers();
// Регистрируем сервис для генерации OpenAPI-документации (Swagger) в среде разработки
builder.Services.AddEndpointsApiExplorer();
// Добавляем генератор Swagger (UI страница с описанием API)
builder.Services.AddSwaggerGen();

// Регистрируем контекст базы данных AppDbContext с использованием SQL Server
// Строка подключения берётся из appsettings.json по ключу "DefaultConnection"
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Читаем секретный ключ для подписи JWT из конфигурации (appsettings.json)
// Если ключ не задан – используем резервный (для разработки, но в проде так не делать)
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "fallback_key_32bytes_!");

// Добавляем аутентификацию через JWT Bearer (схема по умолчанию)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Настройки проверки параметров токена
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Проверять, что ключ подписи действителен
            ValidateIssuerSigningKey = true,
            // Указываем тот же ключ, который использовался при создании токена
            IssuerSigningKey = new SymmetricSecurityKey(key),

            // Проверять издателя токена (iss)
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            // Проверять аудиторию (aud) – для кого предназначен токен
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // Проверять, не истёк ли срок действия токена
            ValidateLifetime = true
        };
    });

// Добавляем политику CORS (Cross-Origin Resource Sharing) – разрешаем запросы с любых доменов,
// любыми методами (GET, POST и т.д.) и любыми заголовками. Это нужно для доступа из браузера
// (для фронтенда).
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Завершаем этап настройки сервисов – строим приложение
var app = builder.Build();

// Создаём отдельную область (scope) для доступа к сервисам вне конвейера запросов
using (var scope = app.Services.CreateScope())
{
    // Получаем экземпляр AppDbContext через DI
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Автоматически применяем все ожидающие миграции к базе данных
    db.Database.Migrate();
    // Заполняем БД начальными данными (роли, пользователи, статусы, тестовые заказы)
    SeedData.Initialize(db);
}

// Если приложение запущено в режиме разработки (Development)
if (app.Environment.IsDevelopment())
{
    // Добавляем middleware для генерации Swagger JSON-спецификации
    app.UseSwagger();
    // Добавляем UI-интерфейс Swagger для удобного тестирования API
    app.UseSwaggerUI();
}

// Перенаправляем HTTP-запросы на HTTPS (для безопасности)
app.UseHttpsRedirection();
// Включаем политику CORS "AllowAll", разрешённую ранее
app.UseCors("AllowAll");
// Включаем аутентификацию (проверяет JWT-токен в заголовке Authorization)
app.UseAuthentication();
// Включаем авторизацию (проверяет права доступа на основе [Authorize] атрибутов)
app.UseAuthorization();
// Маппим маршруты на контроллеры (все атрибуты [Route] и [ApiController])
app.MapControllers();
// Разрешаем отдачу статических файлов по умолчанию (index.html и др.) – для SPA или статической фронтенд-части
app.UseDefaultFiles();
// Включаем обработку статических файлов (CSS, JS, изображения) из папки wwwroot
app.UseStaticFiles();

// Запускаем приложение – начинаем слушать HTTP-запросы
app.Run();