using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Data;
using OnlineShop.Api.Domain.Entities;
using OnlineShop.Api.DTOs;
using System.Security.Cryptography.X509Certificates;


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
        public async Task<int> CreateProductAsync(CreateProductRequestDto request, CancellationToken ct = default)
        {
            var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct);
            if (!categoryExists)
            {
                throw new InvalidOperationException("Category not found.");
            }
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                ImageUrl = request.ImageUrl,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow
            };
            _db.Products.Add(product);
            await _db.SaveChangesAsync(ct);
            return product.Id;
        }

        public async Task UpdateProductAsync(int id, UpdateProductRequestDto request, CancellationToken ct = default)
        {
            var product = await _db.Products.FindAsync(new object[] { id }, ct);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found");
            }
            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            product.ImageUrl = request.ImageUrl;
            product.CategoryId = request.CategoryId;

            await _db.SaveChangesAsync(ct);
        }
        public async Task DeleteProductAsync(int id, CancellationToken ct = default)
        {
            var product = await _db.Products.FindAsync(new object[] { id }, ct);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found");
            }
            var hasOrder = await _db.OrderItems.AnyAsync(c => c.ProductId == id, ct);
            if (hasOrder)
            {
                throw new InvalidOperationException("Cannot delete a product that has been ordered.");
            }
            _db.Products.Remove(product);
            await _db.SaveChangesAsync(ct);
        }
    }
}
