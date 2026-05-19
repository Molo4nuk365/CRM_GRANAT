
namespace CRM_Granat.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Type { get; set; }
        public int? ProductId { get; set; }
        public int? RepairId { get; set; }
        public string Status { get; set; }
        public int? ManagerId { get; set; }
        public int? JewelerId { get; set; }
        public DateTime Deadline { get; set; }
        public decimal Total { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string PaymentStatus { get; set; }
    }
}
