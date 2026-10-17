namespace OnlineShop.Api.DTOs
{
    public record ProductDto (
        int Id,
        string Name,
        string Description,
        decimal Price,
        int StockQuantity,
        string CategoryName);
}
