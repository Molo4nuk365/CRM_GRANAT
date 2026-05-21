// Пространство имён для DTO (Data Transfer Objects) – объекты для передачи данных между клиентом и сервером
namespace CRM_Jewelry_workshop.DTOs;

// DTO для позиции в корзине (товар или ремонт) – используется при создании заказа
public class CartItemDto
{
    // Тип позиции: "product" – готовое изделие (товар), "repair" – ремонт
    // Значение по умолчанию – "product"
    public string Type { get; set; 
    } = "product";

    // Идентификатор товара (если Type == "product") – для ремонта игнорируется
    public int Id { get; set; 
    }
 // Количество единиц (для товара – штуки, для ремонта – количество изделий)
 // Значение по умолчанию – 1
    public int Quantity { get; set; } = 1;
}

// DTO для создания заказа – содержит список позиций (корзину)
public class CreateOrderDto
{
// Список элементов заказа (товары и/или ремонты)
    public List<CartItemDto> Items { get; set; } = new();
}