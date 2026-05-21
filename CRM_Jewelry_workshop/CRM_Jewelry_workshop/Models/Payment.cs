// Подключаем пространства имён для атрибутов валидации и работы с внешними ключами
using System.ComponentModel.DataAnnotations;        // Для [Key]
using System.ComponentModel.DataAnnotations.Schema; // Для [ForeignKey]

// Пространство имён, где хранятся модели данных
namespace CRM_Jewelry_workshop.Models;

// Класс Payment представляет платёж, связанный с заказом
public class Payment
{
    // Первичный ключ таблицы Payments
    [Key] // Указывает, что это свойство – уникальный идентификатор записи
    public int PaymentId { get; set; }

    // Внешний ключ: идентификатор заказа, к которому относится платёж
    public int OrderId { get; set; }

    // Навигационное свойство для связи с таблицей Orders
    // Атрибут ForeignKey указывает, что свойство OrderId является внешним ключом
    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; } // Nullable – может быть не загружен при запросе из БД

    // Сумма платежа (в рублях или другой валюте)
    public decimal Amount { get; set; }

    // Дата и время совершения платежа
    public DateTime Date { get; set; }

    // Внешний ключ: идентификатор статуса платежа (pending, paid, refunded и т.д.)
    public int StatusPaymentId { get; set; }

    // Навигационное свойство для связи с таблицей StatusPayments
    [ForeignKey(nameof(StatusPaymentId))]
    public StatusPayment? StatusPayment { get; set; } // Nullable – может быть не загружен
}