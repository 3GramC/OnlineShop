using OnlineShop.Api.DTOs;

namespace OnlineShop.Api.Services
{
    public interface ICartService
    {
        Task AddToCartAsync(int userId, AddToCartRequestDto request, CancellationToken ct= default);
        Task<CartDto> GetCartAsync(int userId, CancellationToken ct= default);
        Task RemoveFromCartAsync(int userId,int cartItemId, CancellationToken ct= default);
    }
}
