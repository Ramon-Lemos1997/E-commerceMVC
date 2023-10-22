
using Domain.Models;


namespace Domain.Interfaces.Produtos
{
    public interface IProdutosInterface
    {
        Task<OperationResultModel> SaveAsync(ProductModel model);
        Task<(OperationResultModel, IEnumerable<Domain.Entities.Produtos>)> GetProductsAsync();
    }

}
