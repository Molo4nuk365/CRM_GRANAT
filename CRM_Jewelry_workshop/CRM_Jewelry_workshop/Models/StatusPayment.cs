using System.ComponentModel.DataAnnotations;

namespace CRM_Jewelry_workshop.Models;

public class StatusPayment
{
    [Key]
    public int StatusPaymentId { get; set; }
    public string Name { get; set; } = string.Empty; // pending, paid, refunded
    public ICollection<Payment>? Payments { get; set; }
}