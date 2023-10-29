using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class FavoriteProducts
    {
        [Key]
        public int Id { get; private set; } // Chave primária

        public int ProductId { get; set; } // Chave estrangeira para o produto

        public string UserId { get; set; } // Chave estrangeira para o usuário

        [ForeignKey("ProductId")]
        public Produtos Produto { get; set; } // Propriedade de navegação para o produto

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } // Propriedade de navegação para o usuário

        // Construtor
        public FavoriteProducts()
        {
        }

        public FavoriteProducts( int productId, string userId)
        {
            ProductId = productId;
            UserId = userId;
        }
    }
}
