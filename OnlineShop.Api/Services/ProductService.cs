using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Data;
using OnlineShop.Api.DTOs;


namespace OnlineShop.Api.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;
        public ProductService(AppDbContext db) => _db = db;
        public async Task<List<ProductDto>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Products.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.Category.Name)).ToListAsync(ct);
        }
    }
}
