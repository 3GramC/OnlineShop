using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.DTOs;
using OnlineShop.Api.Data;
using OnlineShop.Api.Domain.Entities;

namespace OnlineShop.Api.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _db;
        public CartService(AppDbContext db) => _db = db;

        public async Task AddToCartAsync(int userId , AddToCartRequestDto request, CancellationToken ct = default)
        {
            var product = await _db.Products.FindAsync(request.ProductId, ct);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }
            if (product.StockQuantity < request.Quantity)
            {
                throw new InvalidOperationException("Not enough stock available.");
            }
            var existingItem = await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId, ct);

            if (existingItem != null)
            {
                if (existingItem.Quantity + request.Quantity > 10)
                {
                    throw new InvalidOperationException("The total quantity for this product cannot exceed 10.");
                }
                else
                {
                    existingItem.Quantity += request.Quantity;
                }
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                };
                _db.CartItems.Add(cartItem);
            }
            await _db.SaveChangesAsync();
        }
        public async Task<CartDto> GetCartAsync(int userId , CancellationToken ct = default)
        {
            var cartItems = await _db.CartItems.Where(c => c.UserId == userId).Include(c => c.Product).Select(c => new CartItemDto (
                c.Id,
                c.ProductId,
                c.Product.Name,
                c.Product.Price,
                c.Quantity,
                c.Quantity * c.Product.Price)).ToListAsync(ct);

            var totalAmount = cartItems.Sum(c => c.TotalPrice);
            return new CartDto(cartItems , totalAmount);
        }
        public async Task RemoveFromCartAsync(int userId ,int cartItemId, CancellationToken ct = default)
        {
            var cartItem = await _db.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId, ct);
            if (cartItem == null)
            {
                throw new InvalidOperationException("Cart item not found.");
            }
            _db.CartItems.Remove(cartItem);
            await _db.SaveChangesAsync();
        }
    }
}
