using Domain.Models;
using Newtonsoft.Json;
using Stripe;
using System.Security.Claims;

namespace Domain.Interfaces.Payment
{
    public interface IStripeInterface
    {      
        Task<(OperationResultModel, string url)> GetStripeCheckoutUrl(IEnumerable<int> ordersId, decimal totalAmout);
    }

}
