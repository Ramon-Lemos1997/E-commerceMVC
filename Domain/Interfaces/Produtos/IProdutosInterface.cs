
using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Domain.Interfaces.Produtos
{
    public interface IProdutosInterface
    {
        Task<OperationResultModel> SaveAsync(ProductModel model);
        Task<OperationResultModel> EditAsync(EditProductModel model);
        Task<OperationResultModel> DeleteAsync(int productId);
        Task<(OperationResultModel, IEnumerable<Entities.Produtos>, IEnumerable<FavoriteProducts>)> GetProductsAsync(ClaimsPrincipal user, string? category, string? searchString, int? page);
        Task<(OperationResultModel, Entities.Produtos, IEnumerable<FavoriteProducts>)> GetProductByIdAsync(int id, ClaimsPrincipal user);
        Task<OperationResultModel> AddProductToFavorites(int productId, ClaimsPrincipal user);
        Task<OperationResultModel> RemoveProductFromFavorites(int productId, ClaimsPrincipal user);
        Task<OperationResultModel> AddProductToShoppingCart(int productId, ClaimsPrincipal user);
        Task<OperationResultModel> RemoveProductFromShoppingCart(int productId, ClaimsPrincipal user);
        Task<(OperationResultModel, IEnumerable<ShoppingCartUser>, IEnumerable<FavoriteProducts>)> GetShoppingCartAsync(ClaimsPrincipal user);
        Task<(OperationResultModel, IEnumerable<Entities.Produtos>)> GetFavoriteCardAsync(ClaimsPrincipal user);
        Task<(OperationResultModel, IEnumerable<Entities.Produtos>)> GetAllProductsForAdminAsync(ClaimsPrincipal user);
        Task<(OperationResultModel, EditProductModel model, string pathImage)> GetProductForEditAndDeleteAsync(int id);
        Task<OperationResultModel> UpdateImageAsync(int productId, IFormFile image);
        Task<(OperationResultModel, Dictionary<Entities.Produtos, Order>)> GetProductsPaid(ClaimsPrincipal user);

    }

}
