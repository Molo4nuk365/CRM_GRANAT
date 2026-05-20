using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRM_Jewelry_workshop.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;   // кольцо, серьги и т.д.
        public string Description { get; set; } = string.Empty;
        public decimal Weight { get; set; }               // в граммах
        public decimal Price { get; set; }
        public string Status { get; set; } = "available"; // available, out_of_stock
        public string ImageUrl { get; set; } = string.Empty;
        public string Article { get; set; } = string.Empty;
    }
}
