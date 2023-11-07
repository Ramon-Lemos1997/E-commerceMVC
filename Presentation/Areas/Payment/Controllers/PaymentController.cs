using Domain.Interfaces.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.Payment.Controllers
{
    [Area("Payment")]
    public class PaymentController : Controller
    {
        private readonly IPaymentInterface _paymentService;
        public PaymentController(IPaymentInterface paymentService)
        {
            _paymentService = paymentService;
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
                ModelState.AddModelError(string.Empty, result.Message);
                return RedirectToAction(nameof(Error));
            }

            return Redirect(url);
        }


    //-------------------------------------------------------------------------------------------------------------------------

    }
}
