namespace OnlineShop.Api.DTOs
{
    public record CartItemDto(

        int Id,
        int ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        decimal TotalPrice
     );
}
