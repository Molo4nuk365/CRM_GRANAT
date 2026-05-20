
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Jewelry_workshop.Models;

public class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public int OrderId { get; set; }
    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

    public int StatusPaymentId { get; set; }

    [ForeignKey(nameof(StatusPaymentId))]
    public StatusPayment? StatusPayment { get; set; }
}