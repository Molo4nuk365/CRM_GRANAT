namespace CRM_Granat.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Material { get; set; }
        public string Weight { get; set; }
        public string Article { get; set; }
        public string ImageUrl { get; set; }
    }
}
