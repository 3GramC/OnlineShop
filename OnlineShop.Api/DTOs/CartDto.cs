namespace OnlineShop.Api.DTOs
{
    public record CartDto(
        List<CartItemDto> Items,
        decimal TotalAmount
        );
}
