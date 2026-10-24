using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Api.DTOs
{
    public class AddToCartRequestDto
    {
        [Required(ErrorMessage = "Product Id required")]
        [Range(1,int.MaxValue, ErrorMessage ="ProductId must be greater than 0")]
        public int ProductId { get; set; }
        [Required(ErrorMessage ="Quantity is required")]
        [Range(1,10,ErrorMessage ="Quantity must be between 1 and 10.")]
        public int Quantity { get; set; }
    }
}
