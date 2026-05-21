// Пространство имён для DTO (Data Transfer Objects) – объекты для передачи данных между клиентом и сервером
namespace CRM_Jewelry_workshop.DTOs;

// DTO для обновления статуса заказа (используется в OrdersController.UpdateStatus)
public class UpdateStatusDto
{
    // Свойство Status – новое название статуса (например, "new", "in_progress", "completed", "cancelled")
    // Инициализируем пустой строкой, чтобы избежать предупреждений о nullable-типе
    public string Status { get; set; } = string.Empty;
}
