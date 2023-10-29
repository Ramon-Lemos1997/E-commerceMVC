
using Domain.Models;
using System.Security.Claims;

namespace Domain.Interfaces.Produtos
{
    public interface IProdutosInterface
    {
        Task<OperationResultModel> SaveAsync(ProductModel model);
        Task<(OperationResultModel, IEnumerable<Entities.Produtos>)> GetProductsAsync(string? category, string? searchString, int? page);
        Task<(OperationResultModel, Entities.Produtos)> GetProductByIdAsync(int id);
        Task<OperationResultModel> AddProductToFavorites(int productId, ClaimsPrincipal user);
        Task<(OperationResultModel, IEnumerable<ShoppingCartUser>)> GetShoppingCartAsync(ClaimsPrincipal user);
    }

}
