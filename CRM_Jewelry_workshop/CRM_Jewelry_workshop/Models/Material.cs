// Подключаем пространство имён для атрибутов валидации (в частности, для [Key])
using System.ComponentModel.DataAnnotations;

// Пространство имён, где хранятся модели данных приложения
namespace CRM_Jewelry_workshop.Models;

// Класс Material представляет материал, используемый в производстве ювелирных изделий (металлы, камни и т.д.)
public class Material
{
    // Первичный ключ таблицы Materials
    [Key]// Атрибут указывает, что свойство является уникальным идентификатором записи
    public int MaterialId { get; set; }

    // Название материала (например из карточки, "Серебро 925", "Гранат 0.8 кар" и т.д.)
    public string Name { get; set; } = string.Empty; // Инициализация пустой строкой (non-nullable reference type)

    // Полное описание материала (характеристики, проба, происхождение и т.п.)
    public string Description { get; set; } = string.Empty;

    // Единица измерения (г – граммы, кар – караты, шт – штуки)
    public string Unit { get; set; } = string.Empty;

    // Цена за единицу материала (например, за 1 грамм серебра или за 1 карат граната)
    public decimal PricePerUnit { get; set; }

    // Текущее количество материала на складе (в указанных единицах измерения)
    public decimal QuantityInStock { get; set; }

    // Навигационное свойство: один материал может использоваться во многих позициях (Positions) заказов
    // ICollection<Position> – коллекция объектов Position, которые ссылаются на этот материал
    // Знак ? означает, что коллекция может быть не загружена (зависит от Include при запросе)
    public ICollection<Position>? Positions { get; set; }
}