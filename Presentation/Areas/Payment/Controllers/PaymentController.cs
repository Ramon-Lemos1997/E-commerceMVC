using Domain.Interfaces.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace Presentation.Areas.Payment.Controllers
{
    [Area("Payment")]
    public class PaymentController : Controller
    {
        private readonly IPaymentInterface _paymentService;
        private readonly ILogger<PaymentController> _logger;
        public PaymentController(IPaymentInterface paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        //_______________________________________________________________________________________

        [Authorize]
        [HttpGet]
        public IActionResult Cancel() => View();

        [Authorize]
        [HttpGet]
        public IActionResult Error() => View();

        [Authorize]
        [HttpGet]
        public IActionResult Success() => View();


        //----------------------------------------------------------------------------------------

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateOrder(IEnumerable<int> productsId, IEnumerable<int> productsQuantity)
        {
            var (result, url) = await _paymentService.CreateOrderForPayAsync(User, productsId, productsQuantity);

            if (!result.Success)
            {
                TempData["MessageError"] = result.Message;
                return RedirectToAction(nameof(Error));
            }

            return Redirect(url);
        }

        [HttpPost]
        public async Task HandleOrder([FromBody] int orderId)
        {
            var result = await _paymentService.UpdateOrderAsync(orderId);

            if (!result.Success)
            {
                _logger.LogError($"Erro ao processar pedido com ID {orderId}: {result.Message}");
            }

            else
            {
                _logger.LogWarning($"Pedido com ID {orderId} processado com sucesso.");
            }

        }






        //-------------------------------------------------------------------------------------------------------------------------

    }
}
