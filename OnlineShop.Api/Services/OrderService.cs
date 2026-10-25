using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Data;
using OnlineShop.Api.Domain.Entities;
using OnlineShop.Api.DTOs;
using System.Linq;

namespace OnlineShop.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        public OrderService(AppDbContext db) => _db = db;

        public async Task<int> CreateOrderAsync(int userId, CreateOrderRequestDto request, CancellationToken ct = default)
        {
            using var transaction =await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var cartItems =await _db.CartItems.Include(c => c.Product).Where(c => c.UserId == userId).ToListAsync(ct);
                if (!cartItems.Any())
                {
                    throw new InvalidOperationException("Cart is empty.");
                }
                var order = new Order
                {
                    UserId = userId,
                    Status = Domain.Enums.OrderStatus.Pending,
                    ShippingAddress = request.ShippingAddress,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = 0

                };
                _db.Orders.Add(order);
                await _db.SaveChangesAsync(ct);

                decimal totalAmount = 0;

                foreach (var cartItem in cartItems)
                {
                    if (cartItem.Quantity > cartItem.Product.StockQuantity)
                    {
                        throw new InvalidOperationException($"Not enough stock for {cartItem.Product.Name}");
                    }

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Product.Price
                    };
                    _db.OrderItems.Add(orderItem);
                    totalAmount += (orderItem.Quantity * orderItem.UnitPrice);
                    cartItem.Product.StockQuantity -= orderItem.Quantity;
                }
                order.TotalAmount = totalAmount;
                _db.CartItems.RemoveRange(cartItems);
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return order.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
        public async Task<List<OrderDto>> GetUserOrdersAsync(int userId, CancellationToken ct = default)
        {
            return await _db.Orders.Where(o => o.UserId == userId).Include(o => o.Items).ThenInclude(o => o.Product).OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderDto
                (o.Id,o.OrderDate,o.TotalAmount,o.Status.ToString(),o.ShippingAddress,o.Items.Select(i => new OrderItemDto
                (i.ProductId,i.Product.Name,i.Quantity,i.UnitPrice,i.UnitPrice * i.Quantity)).ToList()
                )).ToListAsync(ct);
        }
    }
}
