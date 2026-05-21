// Подключаем пространство имён для работы с MVC и контроллерами
using Microsoft.AspNetCore.Mvc;
// Подключаем пространство имён для работы с Claims (утверждениями пользователя, извлечёнными из JWT-токена)
using System.Security.Claims;

// Пространство имён для контроллеров API
namespace CRM_Jewelry_workshop.Controllers;

// Атрибут ApiController – включает автоматическую валидацию модели, привязку [FromBody] и другие API-фичи
[ApiController]
// Базовый маршрут: все методы будут доступны по /api/[controller] (но этот абстрактный класс не имеет своих эндпоинтов)
[Route("api/[controller]")]
// Абстрактный класс BaseController – служит базой для всех контроллеров API
// Наследники получат общую функциональность (например, получение ID и роли текущего пользователя)
public abstract class BaseController : ControllerBase
{
  // Свойство только для чтения – возвращает идентификатор текущего пользователя (User ID)
 // Protected – доступно только внутри класса и его наследников
    protected int CurrentUserId =>
        // Находим claim (утверждение) с типом NameIdentifier (обычно это ID пользователя)
        // Если такого claim нет – используем "0"
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // Свойство только для чтения – возвращает роль текущего пользователя (admin, manager, jeweler, client)
    // Protected – доступно только внутри класса и его наследников
    protected string CurrentUserRole =>
        // Находим claim с типом Role (роль пользователя)
        // Если роль не найдена – возвращаем пустую строку
        User.FindFirst(ClaimTypes.Role)?.Value ?? "";
}