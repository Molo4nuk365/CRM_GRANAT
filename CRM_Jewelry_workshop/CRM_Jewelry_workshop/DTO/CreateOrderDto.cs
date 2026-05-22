namespace CRM_Jewelry_workshop.DTOs;

// DTO для позиции в корзине (товар или ремонт)
public class CartItemDto
{
    public string Type { get; set; } = "product";
    public int Id { get; set; }
    public int Quantity { get; set; } = 1;
}

// DTO для создания заказа
public class CreateOrderDto
{
    public int? ClientId { get; set; }   // <-- вот оно, здесь
    public List<CartItemDto> Items { get; set; } = new();
}