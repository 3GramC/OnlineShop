using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Api.DTOs
{
    public class CreateOrderRequestDto
    {
        [Required (ErrorMessage ="Shipping address required")]
        [MinLength(15 ,ErrorMessage ="Shipping address must be at least 15 character")]
        public string ShippingAddress { get; set; } = string.Empty;
    }
}
