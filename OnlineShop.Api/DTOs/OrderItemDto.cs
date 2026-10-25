namespace OnlineShop.Api.DTOs
{
    public record OrderItemDto
    (
        int ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice
    );
}
