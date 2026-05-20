namespace CRM_Jewelry_workshop.DTOs;

public class CartItemDto
{
    public string Type { get; set; } = "product";
    public int Id { get; set; }
    public int Quantity { get; set; } = 1;
}

public class CreateOrderDto
{
    public List<CartItemDto> Items { get; set; } = new();
}