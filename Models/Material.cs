namespace CRM_Granat.Models
{
    public class Material
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal PricePerUnit { get; set; }
    }
}
