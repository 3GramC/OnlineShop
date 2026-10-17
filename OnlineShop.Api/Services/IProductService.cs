using OnlineShop.Api.DTOs;

namespace OnlineShop.Api.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
