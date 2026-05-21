// Подключаем пространства имён для атрибутов валидации и внешних ключей
using System.ComponentModel.DataAnnotations; // Для [Key]
using System.ComponentModel.DataAnnotations.Schema; // Для [ForeignKey]

// Пространство имён моделей приложения
namespace CRM_Jewelry_workshop.Models;

// Класс Order представляет заказ в системе (связывает клиента, менеджера, ювелира, администратора)
public class Order
{
    // Первичный ключ таблицы Orders
    [Key]  // Атрибут – уникальный идентификатор записи
    public int OrderId { get; set; }

    // Внешний ключ: идентификатор клиента (пользователь с ролью client)
    public int ClientId { get; set; }

    // Навигационное свойство: ссылка на объект User (клиент)
    [ForeignKey(nameof(ClientId))] // Указываем, что ClientId – внешний ключ
    public User? Client { get; set; } // Nullable – может быть не загружен при запросе

    // Внешний ключ: идентификатор менеджера (пользователь с ролью manager)
    // int? означает, что менеджер может быть не назначен (nullable)
    public int? ManagerId { get; set; }

    // Навигационное свойство: ссылка на менеджера
    [ForeignKey(nameof(ManagerId))]
    public User? Manager { get; set; }

    // Внешний ключ: идентификатор ювелира (пользователь с ролью jeweler)
    public int? JewelerId { get; set; }

    // Навигационное свойство: ссылка на ювелира
    [ForeignKey(nameof(JewelerId))]
    public User? Jeweler { get; set; }

    // Внешний ключ: идентификатор администратора (пользователь с ролью admin)
    public int? AdminId { get; set; }

    // Навигационное свойство: ссылка на администратора
    [ForeignKey(nameof(AdminId))]
    public User? Admin { get; set; }

    // Внешний ключ: идентификатор статуса заказа (new, in_progress, completed, cancelled)
    public int StatusOrderId { get; set; }

    // Навигационное свойство: ссылка на объект StatusOrder
    [ForeignKey(nameof(StatusOrderId))]
    public StatusOrder? StatusOrder { get; set; }

    // Дата и время создания заказа
    public DateTime CreateDate { get; set; }

    // Общая стоимость заказа (рассчитывается на основе позиций)
    public decimal TotalCost { get; set; }

    // Крайний срок выполнения заказа (может быть null, если не задан)
    public DateTime? Deadline { get; set; }

    // Навигационное свойство: коллекция позиций (строк) заказа
    // Один заказ может содержать много позиций (материалы с количеством)
    public ICollection<Position>? Positions { get; set; }

    // Навигационное свойство: коллекция платежей по заказу
    public ICollection<Payment>? Payments { get; set; }
}