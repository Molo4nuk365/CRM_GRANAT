using Microsoft.AspNetCore.Mvc;          
// Базовые классы для контроллеров: ControllerBase, ApiController, Route и т.д.
using System.Security.Claims;            
// Для работы с утверждениями (claims) аутентифицированного пользователя

namespace CRM_Jewelry_workshop.Controllers;

// Абстрактный базовый контроллер для всех контроллеров приложения.
// Наследует ControllerBase (лёгкий вариант без поддержки представлений) и содержит
// общие свойства для получения ID и роли текущего пользователя из ClaimsPrincipal.
[ApiController]                    
// Помечает класс как контроллер API (автоматическая валидация модели, привязка)
[Route("api/[controller]")]        
// Задаёт шаблон маршрута; фактический маршрут определяется в наследниках
public abstract class BaseController : ControllerBase
{
    // Свойство, возвращающее идентификатор текущего аутентифицированного пользователя.
    // Извлекается из claim'а типа ClaimTypes.NameIdentifier (это userId).
    // Если claim отсутствует, возвращается 0 (например, для неаутентифицированного пользователя).
    protected int CurrentUserId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // Свойство, возвращающее роль текущего пользователя.
    // Извлекается из claim'а типа ClaimTypes.Role (например, "admin", "client").
    // Если роль не задана, возвращается пустая строка.
    protected string CurrentUserRole =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? "";
}