using OnlineShop.Api.DTOs;

namespace OnlineShop.Api.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<int> CreateProductAsync(CreateProductRequestDto request, CancellationToken ct = default);
        Task UpdateProductAsync(int id, UpdateProductRequestDto request, CancellationToken ct = default);
        Task DeleteProductAsync(int id, CancellationToken ct = default);
    }
}
