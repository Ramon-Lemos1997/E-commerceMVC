using Domain.Models;
using System.Security.Claims;

namespace Domain.Interfaces.Payment
{
    public interface IPaymentInterface
    {
        Task<(OperationResultModel, string url)> CreateOrderForPayAsync(ClaimsPrincipal user, IEnumerable<int> productsId, IEnumerable<int> productsQuantity);
        Task<OperationResultModel> UpdateOrderAsync(int orderId);
    }
}
