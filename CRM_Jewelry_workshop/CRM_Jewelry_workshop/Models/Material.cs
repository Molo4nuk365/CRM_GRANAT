using System.ComponentModel.DataAnnotations;

namespace CRM_Jewelry_workshop.Models;

public class Material
{
    [Key]
    public int MaterialId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;     // г, кар, шт
    public decimal PricePerUnit { get; set; }
    public decimal QuantityInStock { get; set; }
    public ICollection<Position>? Positions { get; set; }
}