// Подключаем системные пространства имён (для базовых типов, атрибутов валидации и работы с БД)
using System;
using System.ComponentModel.DataAnnotations.Schema;   
// Для атрибутов, связанных с отображением в БД (например, ForeignKey, Column)
using System.ComponentModel.DataAnnotations;        
// Для атрибутов валидации и первичного ключа [Key]

// Объявляем пространство имён, где находятся модели приложения
namespace CRM_Jewelry_workshop.Models
{
    // Товар – ювелирное изделие
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        // Наименование изделия (например, "Кольцо «Гранатовый рассвет»")
        public string Name { get; set; } = string.Empty;

        // Тип: кольцо, серьги, подвеска, браслет, брошь
        public string Type { get; set; } = string.Empty;

        // Полное описание (материал, особенности, страна производства)
        public string Description { get; set; } = string.Empty;

        // Вес в граммах (грамовка)
        public decimal Weight { get; set; }

        // Металл, из которого изготовлено изделие (например, "Серебро 925")
        public string Metal { get; set; } = "Серебро 925";

        // Камень-вставка (например, "Гранат")
        public string Stone { get; set; } = "Гранат";

        // Цена в рублях
        public decimal Price { get; set; }

        // Статус наличия: available / out_of_stock
        public string Status { get; set; } = "available";

        // URL изображения товара (относительный путь внутри wwwroot)
        public string ImageUrl { get; set; } = string.Empty;

        // Артикул – уникальный код товара
        public string Article { get; set; } = string.Empty;
    }
}