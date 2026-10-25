namespace OnlineShop.Api.DTOs
{
    public record OrderDto
        (
            int Id,
            DateTime OrderDate,
            decimal TotalAmount,
            string Status,
            string ShippingAddress,
            List<OrderItemDto> Items
        );
}
