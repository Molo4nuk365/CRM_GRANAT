// Подключаем системные пространства имён (для базовых типов, атрибутов валидации и работы с БД)
using System;
using System.ComponentModel.DataAnnotations.Schema;   // Для атрибутов, связанных с отображением в БД (например, ForeignKey, Column)
using System.ComponentModel.DataAnnotations;        // Для атрибутов валидации и первичного ключа [Key]

// Объявляем пространство имён, где находятся модели приложения
namespace CRM_Jewelry_workshop.Models
{
    // Класс Product представляет товар (ювелирное изделие) в каталоге
    public class Product
    {
        // Первичный ключ – уникальный идентификатор товара
        [Key] // Атрибут указывает, что свойство является первичным ключом в таблице Products
        public int ProductId { get; set; }

        // Название изделия (например, "Кольцо «Гранатовый рассвет»")
        public string Name { get; set; } = string.Empty;   // Инициализация пустой строкой (non-nullable)

        // Тип изделия (кольцо, серьги, подвеска, браслет, брошь)
        public string Type { get; set; } = string.Empty;

        // Полное описание товара (материал, вставки, размер, особенности)
        public string Description { get; set; } = string.Empty;

        // Вес изделия в граммах (тип decimal для точности)
        public decimal Weight { get; set; }

        // Цена товара в рублях или другой валюте
        public decimal Price { get; set; }

        // Статус наличия на складе (available – в наличии, out_of_stock – нет в наличии)
        // Значение по умолчанию "available"
        public string Status { get; set; } = "available";

        // URL (путь) к изображению товара (которое есть на сайте)
        public string ImageUrl { get; set; } = string.Empty;

        // Артикул товара – уникальный код для идентификации в учётной системе
        public string Article { get; set; } = string.Empty;
    }
}