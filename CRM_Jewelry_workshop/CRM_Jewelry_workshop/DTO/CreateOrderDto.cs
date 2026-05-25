using CRM_Jewelry_workshop.Models;
using CRM_Jewelry_workshop.DTO;

namespace CRM_Jewelry_workshop.DTO;

// Одна позиция корзины (товар или ремонт)
public class CartItemDto
{
    public string Type { get; set; } = "product";   // "product" или "repair"
    public int Id { get; set; }                     // ProductId или RepairId
    public int Quantity { get; set; } = 1;
}

// DTO для создания заказа (клиент передаёт только позиции)
public class CreateOrderDto
{
    // Поле ClientId НЕ передаётся – заказ всегда для текущего клиента
    public List<CartItemDto> Items { get; set; } = new();
}