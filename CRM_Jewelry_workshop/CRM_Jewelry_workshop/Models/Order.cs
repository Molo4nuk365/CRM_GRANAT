using SharpDX.Direct3D9;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Jewelry_workshop.Models;

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

    public decimal Price { get; set; } // цена материала на момент заказа
    public decimal Quantity { get; set; }
}