using System.ComponentModel.DataAnnotations;

namespace CRM_Jewelry_workshop.Models;

public class StatusOrder
{
    [Key]
    public int StatusOrderId { get; set; }
    public string Name { get; set; } = string.Empty; // new, in_progress, completed, cancelled
    public ICollection<Order>? Orders { get; set; }
}