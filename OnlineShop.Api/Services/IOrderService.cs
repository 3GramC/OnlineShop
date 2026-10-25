using OnlineShop.Api.DTOs;

namespace OnlineShop.Api.Services
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(int userId, CreateOrderRequestDto request, CancellationToken ct = default);
        Task <List<OrderDto>> GetUserOrdersAsync(int userId, CancellationToken ct = default);
    }
}
