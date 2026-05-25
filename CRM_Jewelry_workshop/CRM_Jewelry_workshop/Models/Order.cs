// Подключаем пространства имён для атрибутов валидации и внешних ключей
using System.ComponentModel.DataAnnotations;
// Для [Key]
using System.ComponentModel.DataAnnotations.Schema; 
// Для [ForeignKey]

// Пространство имён моделей приложения
namespace CRM_Jewelry_workshop.Models;

// Заказ, созданный клиентом
public class Order
{
    [Key]
    public int OrderId { get; set; }

    // Клиент, оформивший заказ (обязательный)
    public int ClientId { get; set; }
    [ForeignKey(nameof(ClientId))]
    public User? Client { get; set; }

    // Менеджер, ответственный за заказ (может быть не назначен)
    public int? ManagerId { get; set; }
    [ForeignKey(nameof(ManagerId))]
    public User? Manager { get; set; }

    // Ювелир, выполняющий заказ (может быть не назначен)
    public int? JewelerId { get; set; }
    [ForeignKey(nameof(JewelerId))]
    public User? Jeweler { get; set; }

    // Администратор, который контролирует заказ (опционально)
    public int? AdminId { get; set; }
    [ForeignKey(nameof(AdminId))]
    public User? Admin { get; set; }

    // Статус заказа (new, in_progress, completed, cancelled)
    public int StatusOrderId { get; set; }
    [ForeignKey(nameof(StatusOrderId))]
    public StatusOrder? StatusOrder { get; set; }

    // Дата и время создания заказа
    public DateTime CreateDate { get; set; }

    // Общая стоимость заказа (рассчитывается автоматически)
    public decimal TotalCost { get; set; }

    // Плановый срок выполнения (может быть не задан)
    public DateTime? Deadline { get; set; }

    // Позиции заказа – список материалов, используемых в заказе
    public ICollection<Position>? Positions { get; set; }

    // Платежи по этому заказу (один заказ может оплачиваться частями)
    public ICollection<Payment>? Payments { get; set; }
}