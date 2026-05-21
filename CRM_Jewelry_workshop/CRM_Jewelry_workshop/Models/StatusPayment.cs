// Подключаем пространство имён для атрибутов валидации (например, [Key])
using System.ComponentModel.DataAnnotations;

// Объявляем пространство имён, соответствующее папке Models проекта
namespace CRM_Jewelry_workshop.Models;

// Класс StatusPayment представляет статус платежа (например, ожидает, оплачен, возвращён)
public class StatusPayment
{
 // Первичный ключ таблицы StatusPayments в базе данных
    [Key] // Атрибут указывает, что свойство является уникальным идентификатором
    public int StatusPaymentId { get; set; }

 // Название статуса (pending, paid, refunded) – обязательное поле
 // Инициализируем пустой строкой, чтобы избежать предупреждений о nullable-типе
    public string Name { get; set; } = string.Empty;

 // Навигационное свойство: один статус может быть у многих платежей
 // ICollection<Payment> – коллекция объектов Payment, связанных с этим статусом
 // Nullable (?) означает, что коллекция может быть не загружена (например, при запросе без Include)
    public ICollection<Payment>? Payments { get; set; }
}