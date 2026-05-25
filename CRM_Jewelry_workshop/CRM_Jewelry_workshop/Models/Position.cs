 
using System.ComponentModel.DataAnnotations; 
// Для атрибутов валидации ([Key])
using System.ComponentModel.DataAnnotations.Schema; 
// Для атрибута [ForeignKey]

// Пространство имён моделей приложения
namespace CRM_Jewelry_workshop.Models;

// Позиция заказа (материал + количество)
public class Position
{
    [Key]
    public int PositionId { get; set; }

    public int OrderId { get; set; }
    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    public int MaterialId { get; set; }
    [ForeignKey(nameof(MaterialId))]
    public Material? Material { get; set; }

    public decimal Price { get; set; }      // Цена на момент заказа
    public decimal Quantity { get; set; }   // Количество материала
}