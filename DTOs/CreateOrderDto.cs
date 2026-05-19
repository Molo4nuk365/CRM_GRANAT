#nullable disable
namespace CRM_Granat.DTOs;

public class CartItemDto
{
    public string Type { get; set; }
    public int Id { get; set; }
    public int Quantity { get; set; }
}

public class CreateOrderDto
{
    public List<CartItemDto> Items { get; set; }
}
