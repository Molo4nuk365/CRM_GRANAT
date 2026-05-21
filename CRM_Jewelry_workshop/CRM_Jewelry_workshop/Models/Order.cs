using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Jewelry_workshop.Models;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    public int ClientId { get; set; }
    [ForeignKey(nameof(ClientId))]
    public User? Client { get; set; }

    public int? ManagerId { get; set; }
    [ForeignKey(nameof(ManagerId))]
    public User? Manager { get; set; }

    public int? JewelerId { get; set; }
    [ForeignKey(nameof(JewelerId))]
    public User? Jeweler { get; set; }

    public int? AdminId { get; set; }
    [ForeignKey(nameof(AdminId))]
    public User? Admin { get; set; }

    public int StatusOrderId { get; set; }
    [ForeignKey(nameof(StatusOrderId))]
    public StatusOrder? StatusOrder { get; set; }

    public DateTime CreateDate { get; set; }
    public decimal TotalCost { get; set; }

    public DateTime? Deadline { get; set; }

    public ICollection<Position>? Positions { get; set; }
    public ICollection<Payment>? Payments { get; set; }
}