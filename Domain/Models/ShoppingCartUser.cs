

namespace Domain.Models
{
    public class ShoppingCartUser
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string PathImage { get; set; }
    }

}
