using OnlineShop.Api.DTOs;

namespace OnlineShop.Api.Services
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequestDto request , CancellationToken ct = default);
    }
}
