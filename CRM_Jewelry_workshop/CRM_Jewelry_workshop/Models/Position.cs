 
using System.ComponentModel.DataAnnotations; // Для атрибутов валидации ([Key])
using System.ComponentModel.DataAnnotations.Schema; // Для атрибута [ForeignKey]

// Пространство имён моделей приложения
namespace CRM_Jewelry_workshop.Models;

// Класс Position представляет позицию (строку) заказа – связь между заказом и материалом с указанием цены и количества
public class Position
{
    // Первичный ключ таблицы Positions
    [Key]// Атрибут – первичный ключ
    public int PositionId { get; set; }

    // Внешний ключ: идентификатор заказа, к которому относится позиция
    public int OrderId { get; set; }

    // Навигационное свойство: ссылка на объект Order (заказ)
    // Атрибут ForeignKey указывает, что свойство OrderId является внешним ключом для этой связи
    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set;}// Nullable – может быть не загружен при запросе

    // Внешний ключ: идентификатор материала, используемого в позиции
    public int MaterialId { get; set; }

    // Навигационное свойство: ссылка на объект Material (материал)
    [ForeignKey(nameof(MaterialId))]
    public Material? Material { get; set; } // Nullable – может быть не загружен

    // Цена материала на момент создания заказа (сохраняется исторически, даже если цена материала изменится)
    public decimal Price { get; set; }

    // Количество материала (в граммах, штуках и т.д.)
    public decimal Quantity { get; set; }
}