using CRM_Jewelry_workshop.DTO;       
// Подключаем DTO-классы: CreateOrderDto (для создания заказа)
using CRM_Jewelry_workshop.Services;   
// Подключаем интерфейс IOrderService — бизнес-логику заказов
using Microsoft.AspNetCore.Authorization; 
// Атрибут [Authorize] для ограничения доступа по ролям
using Microsoft.AspNetCore.Mvc;          
// Базовые классы для контроллеров (ControllerBase, IActionResult и др.)

namespace CRM_Jewelry_workshop.Controllers
{
    // [Authorize] на уровне контроллера означает, что все методы требуют аутентификации.
    // Доступ для анонимных пользователей запрещён, но роли могут уточняться на методах.
    [Authorize]
    [ApiController]                     
    // Включает автоматическую валидацию модели и привязку параметров
    [Route("api/[controller]")]         
    // Базовый маршрут: api/Orders
    public class OrdersController : BaseController 
     // Наследуемся от BaseController, где определены
     // свойства CurrentUserId (Id текущего пользователя)
     // и CurrentUserRole (роль текущего пользователя)
    {
     // Сервис для работы с заказами — внедряется через DI
        private readonly IOrderService _orderService;

    // Внедрение зависимости IOrderService через конструктор
        public OrdersController(IOrderService orderService) => _orderService = orderService;

    // POST api/orders/create — создание нового заказа клиентом.
    // Доступен только клиентам (проверка внутри метода).
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                // Проверяем, что текущая роль — "client", иначе возвращаем 403 Forbidden
                if (CurrentUserRole != "client")
                    return Forbid("Только клиенты могут создавать заказы");

                // Вызываем сервис создания заказа, передавая ID текущего пользователя и DTO
                var order = await _orderService.CreateOrderAsync(CurrentUserId, dto);

                // Возвращаем 200 OK с сообщением и ID созданного заказа
                return Ok(new { message = "Заказ успешно создан. Ожидайте подтверждения менеджера.", orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                // Любая ошибка (например, товар не найден) преобразуется в 400 Bad Request
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/orders — получение списка заказов для текущего пользователя.
        // Фильтрация зависит от роли пользователя (в сервисе).
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            // Получаем список DTO заказов через сервис, передавая ID и роль текущего пользователя
            var orders = await _orderService.GetOrdersForUserAsync(CurrentUserId, CurrentUserRole);
            return Ok(orders); 
            // 200 OK с массивом заказов
        }

        // GET api/orders/{id} — получение конкретного заказа по идентификатору.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            // Загружаем заказ со всеми связанными сущностями через сервис
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound(); 
            // 404 если заказ не найден
            return Ok(order);                      
            // 200 OK с объектом заказа
        }

        // PUT api/orders/{id}/accept — менеджер принимает заказ.
        // Роль не проверяется явно, предполагается, что это доступно менеджеру (логика в сервисе).
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> AcceptOrder(int id)
        {
            try
            {
                // Вызываем метод сервиса, передавая ID заказа и ID текущего пользователя (менеджера)
                var order = await _orderService.AcceptOrderAsync(id, CurrentUserId);
                if (order == null) return NotFound();
                // Заказ не найден — 404
                return Ok(order);                      
                // Успешно — возвращаем обновлённый заказ
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); } // Ошибка логики — 400
        }

        // PUT api/orders/{id}/take — ювелир берёт заказ в работу.
        [HttpPut("{id}/take")]
        public async Task<IActionResult> TakeOrder(int id)
        {
            try
            {
                // Сервис назначает текущего пользователя (ювелира) исполнителем заказа
                var order = await _orderService.TakeOrderAsync(id, CurrentUserId);
                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PUT api/orders/{id}/assignJeweler — менеджер/админ назначает ювелира на заказ.
        // Доступ ограничен ролями admin и manager.
        [Authorize(Roles = "admin,manager")]
        [HttpPut("{id}/assignJeweler")]
        public async Task<IActionResult> AssignJeweler(int id, [FromBody] int jewelerId)
        {
            try
            {
                // Передаём ID заказа и ID выбранного ювелира (приходит в теле запроса)
                var order = await _orderService.AssignJewelerAsync(id, jewelerId);
                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PUT api/orders/{id}/ready — ювелир отмечает заказ как выполненный (готов).
        [HttpPut("{id}/ready")]
        public async Task<IActionResult> ReadyOrder(int id)
        {
            try
            {
                // Сервис меняет статус на "Готов", проверяя, что заказ был в работе
                var order = await _orderService.ReadyOrderAsync(id);
                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PUT api/orders/{id}/complete — менеджер завершает заказ (после выдачи клиенту).
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            try
            {
                // Меняем статус на "Завершён"
                var order = await _orderService.CompleteOrderAsync(id);
                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PUT api/orders/{id}/cancel — отмена заказа.
        // Может вызываться клиентом или менеджером (ограничения по ролям в сервисе).
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                // Меняем статус на "Отменён"
                var order = await _orderService.CancelOrderAsync(id);
                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PUT api/orders/{id}/setDeadline — установка дедлайна заказа.
        // Доступно менеджерам и администраторам (проверка в сервисе + дополнительная обработка).
        [HttpPut("{id}/setDeadline")]
        public async Task<IActionResult> SetDeadline(int id, [FromBody] DateTime deadline)
        {
            try
            {
                // Вызываем сервис, передавая ID заказа, новую дату и роль текущего пользователя
                var order = await _orderService.SetDeadlineAsync(id, deadline, CurrentUserRole);
                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Если сервис выбросил исключение о недостатке прав, возвращаем 403 Forbidden
                return Forbid(ex.Message);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); } // Другие ошибки — 400
        }
    }
}