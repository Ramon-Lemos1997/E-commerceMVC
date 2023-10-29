using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; private set; }
        public int ProductId { get; set; } // Chave estrangeira para o produto
        public string UserId { get; set; } // Chave estrangeira para a tabela de usuários

        [ForeignKey("ProductId")]
        public Produtos Produto { get; set; } // Propriedade de navegação para o produto

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } // Propriedade de navegação para o usuário

        public ShoppingCart()
        {
        }

        public ShoppingCart(int productId, string userId)
        {
            ProductId = productId;
            UserId = userId;
        }
    }
}

