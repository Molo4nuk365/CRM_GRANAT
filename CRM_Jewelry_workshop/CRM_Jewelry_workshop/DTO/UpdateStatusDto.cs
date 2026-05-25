// Пространство имён для DTO (Data Transfer Objects) – объекты для передачи данных между клиентом и сервером
namespace CRM_Jewelry_workshop.DTO;

// DTO для изменения статуса заказа
public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty; // 
}

