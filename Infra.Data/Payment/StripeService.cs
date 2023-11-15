using Azure.Core;
using Domain.Interfaces.Payment;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using static Infra.Data.Stripe.StripeService;

namespace Infra.Data.Stripe
{
    public class StripeService : IStripeInterface
    {
        private readonly IConfiguration _config;
        public StripeService(IConfiguration config)
        {
            _config = config;
        }

        //----------------------------------------------------------------------------------------

        /// <summary>
        /// Gera a URL de checkout para a sessão de pagamento no Stripe com base nos IDs dos pedidos e no valor total.
        /// </summary>
        /// <param name="ordersId">Os IDs dos pedidos para os quais a sessão de pagamento está sendo criada.</param>
        /// <param name="totalAmout">O valor total a ser cobrado na sessão de pagamento.</param>
        /// <returns>Uma tupla contendo um objeto OperationResultModel e a URL da sessão de pagamento, se bem-sucedida.</returns>
        public async Task<(OperationResultModel, string url)> GetStripeCheckoutUrl(IEnumerable<int> ordersId, decimal totalAmout)
        {
            try
            {
                var metadata = new Dictionary<string, string>();

                int counter = 1;
                foreach (var orderId in ordersId)
                {
                    metadata.Add($"orderId{counter}", orderId.ToString());
                    counter++;
                }

                // Converto para long, pois a stripe aceita somente long e não decimal
                long totalAmoutLong = Convert.ToInt64(totalAmout * 100);

                var secretKey = _config["StripeSettings:SecretKey"];

                StripeConfiguration.ApiKey = secretKey;
                var domain = "https://localhost:7034/Payment/Payment";
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>
                    {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = totalAmoutLong,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Total a pagar",
                            },
                        },
                        Quantity = 1,
                    },
                },
                    Mode = "payment",
                    SuccessUrl = domain + "/Success",
                    CancelUrl = domain + "/Cancel",
                    Metadata = metadata
                };

                var service = new SessionService();
                Session session = service.Create(options);
                return (new OperationResultModel(true, "Sessão construída com sucesso."), session.Url);
            }
            catch (Exception ex)
            {
                return (new OperationResultModel(false, $"Exceção não planejada: {ex.Message}"), null);
            }
        }


        //--------------------------------------------------------------------------------------------------------



    }
}

