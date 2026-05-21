// Подключаем пространство имён для атрибутов валидации (в частности, для [Key])
using System.ComponentModel.DataAnnotations;

// Объявляем пространство имён, где находятся модели данных приложения
namespace CRM_Jewelry_workshop.Models;

// Класс StatusOrder представляет статус заказа (например, новый, в работе, выполнен, отменён)
public class StatusOrder
{
 // Первичный ключ таблицы StatusOrders в базе данных
    [Key]  // Атрибут указывает, что свойство является уникальным идентификатором записи
    public int StatusOrderId { get; set; }

    // Название статуса (new, in_progress, completed, cancelled) – текстовое обозначение состояния заказа
    // Инициализируем пустой строкой, чтобы избежать предупреждений о возможном null (non-nullable reference type)
    public string Name { get; set; } = string.Empty;

    // Навигационное свойство: один статус может быть присвоен многим заказам
    // ICollection<Order> – коллекция объектов Order, которые имеют данный статус
    // Знак ? означает, что свойство может быть null (если не загружено через Include при запросе к БД)
    public ICollection<Order>? Orders { get; set; }
}