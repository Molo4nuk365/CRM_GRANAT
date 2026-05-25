using CRM_Jewelry_workshop.Data;      // Контекст базы данных (AppDbContext) — точка входа к таблицам
using CRM_Jewelry_workshop.DTO;        // Объекты передачи данных, используемые в методах сервиса
using CRM_Jewelry_workshop.Models;     // Модели сущностей: Order, StatusOrder, User, Role и др.
using Microsoft.EntityFrameworkCore;  // Для Include, FirstAsync, ToListAsync и прочих методов EF Core

namespace CRM_Jewelry_workshop.Services
{
    // Реализация интерфейса IOrderService — основная бизнес-логика работы с заказами
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db; // Поле для доступа к БД, внедряется через конструктор

        // Внедрение зависимости через конструктор — стандартная практика в ASP.NET Core
        public OrderService(AppDbContext db) => _db = db;

        // Создание нового заказа клиентом
        public async Task<Order> CreateOrderAsync(int clientUserId, CreateOrderDto dto)
        {
            // Находим статус "Новый" — он должен существовать в таблице StatusOrders
            var statusNew = await _db.StatusOrders.FirstAsync(s => s.Name == "Новый");

            // Инициализируем объект заказа с базовыми значениями
            var order = new Order
            {
                ClientId = clientUserId,            
                // Id клиента, создающего заказ
                StatusOrderId = statusNew.StatusOrderId, 
                // Статус "Новый"
                CreateDate = DateTime.Now,         
                // Текущее время как дата создания
                TotalCost = 0,                     
                // Итоговая стоимость пока неизвестна
                ManagerId = null,                  
                // Менеджер ещё не назначен
                JewelerId = null                   
                // Ювелир ещё не назначен
            };

            // Добавляем заказ в контекст (пока только в памяти)
            _db.Orders.Add(order);
            // Сохраняем, чтобы получить OrderId, который может понадобиться далее
            await _db.SaveChangesAsync();

            // Рассчитываем общую стоимость на основе позиций из DTO
            decimal totalCost = 0;
            foreach (var item in dto.Items)
            {
                // Если тип позиции — готовое изделие
                if (item.Type == "product")
                {
                    // Ищем товар по ID; если не найден — ошибка
                    var product = await _db.Products.FindAsync(item.Id);
                    if (product == null) throw new Exception($"Товар с id {item.Id} не найден");
                    totalCost += product.Price * item.Quantity; // Цена товара * количество
                }
                // Если тип позиции — ремонт (услуга)
                else if (item.Type == "repair")
                {
                    totalCost += 3500 * item.Quantity; // Фиксированная стоимость ремонта 3500 за единицу
                }
                else
                {
                    // Неизвестный тип позиции — выбрасываем исключение
                    throw new Exception("Неверный тип позиции");
                }
            }

            // Обновляем итоговую стоимость заказа
            order.TotalCost = totalCost;
            // Сохраняем изменения (обновлённый TotalCost)
            await _db.SaveChangesAsync();
            return order; // Возвращаем созданный заказ со всеми полями
        }

        // Получение списка заказов в зависимости от роли пользователя
        public async Task<List<OrderDto>> GetOrdersForUserAsync(int userId, string role)
        {
            // Базовый запрос с подгрузкой связанных сущностей (клиент, менеджер, ювелир, статус)
            IQueryable<Order> query = _db.Orders
            .Include(o => o.Client)       
            // Чтобы получить имя клиента
            .Include(o => o.Manager)      
            // Имя менеджера
            .Include(o => o.Jeweler)      
            // Имя ювелира
            .Include(o => o.StatusOrder); 
            // Название статуса

            // Фильтрация заказов в зависимости от роли
            query = role switch
            {
                // Администратор и менеджер видят все заказы
                "admin" => query,
                "manager" => query,
                // Ювелир видит заказы, которые либо свободны (JewelerId == null), либо назначены ему
                "jeweler" => query.Where(o => o.JewelerId == null || o.JewelerId == userId),
                // Клиент видит только свои заказы
                "client" => query.Where(o => o.ClientId == userId),
                // Для неизвестной роли — пустой результат
                _ => query.Where(o => false)
            };

            // Преобразуем результат в список DTO с нужными полями
            return await query.Select(o => new OrderDto
            {
                OrderId = o.OrderId,
                ClientName = o.Client != null ? o.Client.FullName : null,
                ManagerName = o.Manager != null ? o.Manager.FullName : null,
                JewelerName = o.Jeweler != null ? o.Jeweler.FullName : null,
                JewelerId = o.JewelerId,
                Status = o.StatusOrder != null ? o.StatusOrder.Name : null,
                TotalCost = o.TotalCost,
                CreateDate = o.CreateDate,
                Deadline = o.Deadline
            }).ToListAsync();
        }

        // Менеджер принимает заказ (переводит из "Новый" в "Принят")
        public async Task<Order?> AcceptOrderAsync(int orderId, int managerId)
        {
            // Загружаем заказ вместе со статусом для проверки
            var order = await _db.Orders
                .Include(o => o.StatusOrder)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return null; // Заказ не найден

            // Принимать можно только заказы в статусе "Новый"
            if (order.StatusOrder?.Name != "Новый")
                throw new Exception("Принимать можно только новые заказы");

            // Находим статус "Принят"
            var statusAccepted = await _db.StatusOrders.FirstAsync(s => s.Name == "Принят");
            // Меняем статус и фиксируем менеджера, принявшего заказ
            order.StatusOrderId = statusAccepted.StatusOrderId;
            order.ManagerId = managerId;
            await _db.SaveChangesAsync();
            return order;
        }

        // Ювелир самостоятельно берёт заказ в работу (переводит из "Принят" в "В работе")
        public async Task<Order?> TakeOrderAsync(int orderId, int jewelerId)
        {
            var order = await _db.Orders
                .Include(o => o.StatusOrder)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return null;

            // Брать можно только принятые заказы (ожидающие исполнителя)
            if (order.StatusOrder?.Name != "Принят")
                throw new Exception("Брать в работу можно только принятые заказы");

            // Находим статус "В работе"
            var statusInWork = await _db.StatusOrders.FirstAsync(s => s.Name == "В работе");
            order.StatusOrderId = statusInWork.StatusOrderId;
            order.JewelerId = jewelerId; // Назначаем себя исполнителем
            await _db.SaveChangesAsync();
            return order;
        }

        // Менеджер (или администратор) назначает конкретного ювелира на заказ
        public async Task<Order?> AssignJewelerAsync(int orderId, int jewelerId)
        {
            var order = await _db.Orders
                .Include(o => o.StatusOrder)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return null;

            // Назначать можно только для заказов со статусом "Новый" или "Принят"
            if (order.StatusOrder?.Name != "Принят" && order.StatusOrder?.Name != "Новый")
                throw new Exception("Назначать ювелира можно только для нового или принятого заказа");

            // Проверяем, что указанный пользователь существует
            var jeweler = await _db.Users.FindAsync(jewelerId);
            if (jeweler == null)
                throw new Exception("Пользователь не найден");

            // Проверяем, что у него роль "jeweler"
            var role = await _db.Roles.FindAsync(jeweler.RoleId);
            if (role?.RoleName != "jeweler")
                throw new Exception("Указанный пользователь не является ювелиром");

            // Назначаем ювелира (статус заказа не меняется)
            order.JewelerId = jewelerId;
            await _db.SaveChangesAsync();
            return order;
        }

        // Ювелир помечает заказ как выполненный (переводит в статус "Готов")
        public async Task<Order?> ReadyOrderAsync(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.StatusOrder)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return null;

            // Отмечать готовность можно только для заказов, находящихся в работе
            if (order.StatusOrder?.Name != "В работе")
                throw new Exception("Отметить готовность можно только для заказа в работе");

            // Находим статус "Готов"
            var statusReady = await _db.StatusOrders.FirstAsync(s => s.Name == "Готов");
            order.StatusOrderId = statusReady.StatusOrderId;
            await _db.SaveChangesAsync();
            return order;
        }

        // Менеджер завершает заказ (переводит из "Готов" в "Завершён")
        public async Task<Order?> CompleteOrderAsync(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.StatusOrder)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return null;

            // Завершить можно только готовый заказ
            if (order.StatusOrder?.Name != "Готов")
                throw new Exception("Завершить можно только готовый заказ");

            // Находим статус "Завершён"
            var statusCompleted = await _db.StatusOrders.FirstAsync(s => s.Name == "Завершён");
            order.StatusOrderId = statusCompleted.StatusOrderId;
            await _db.SaveChangesAsync();
            return order;
        }

        // Отмена заказа (любой роли, если разрешено)
        public async Task<Order?> CancelOrderAsync(int orderId)
        {
            // Загружаем заказ без дополнительных проверок статуса
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return null;

            // Находим статус "Отменён"
            var statusCancelled = await _db.StatusOrders.FirstAsync(s => s.Name == "Отменён");
            order.StatusOrderId = statusCancelled.StatusOrderId;
            await _db.SaveChangesAsync();
            return order;
        }

        // Установка или изменение дедлайна заказа (только для менеджера/админа)
        public async Task<Order?> SetDeadlineAsync(int orderId, DateTime deadline, string currentUserRole)
        {
            // Проверка прав доступа
            if (currentUserRole != "admin" && currentUserRole != "manager")
                throw new UnauthorizedAccessException("Только менеджер или администратор может установить срок");

            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return null;

            // Просто обновляем поле Deadline
            order.Deadline = deadline;
            await _db.SaveChangesAsync();
            return order;
        }

        // Получение заказа по идентификатору со всеми связанными данными
        public async Task<Order?> GetOrderByIdAsync(int orderId) =>
        await _db.Orders
        .Include(o => o.Client)       
         // Для отображения информации о клиенте
        .Include(o => o.Manager)      
          // О менеджере
         .Include(o => o.Jeweler)      
          // О ювелире
         .Include(o => o.StatusOrder) 
          // О текущем статусе
          .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }
}