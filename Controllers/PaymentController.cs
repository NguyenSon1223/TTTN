using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;

namespace Ecommerce.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PayOS _payOS;

        public PaymentController(IConfiguration config)
        {
            var clientId = config["PayOS:ClientId"];
            var apiKey = config["PayOS:ApiKey"];
            var checksumKey = config["PayOS:ChecksumKey"];
            _payOS = new PayOS(clientId, apiKey, checksumKey);
        }

        [HttpGet("create-payment")]
        public async Task<IActionResult> CreatePayment()
        {
            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var items = new List<ItemData>
        {
            new ItemData("Test Product 1", 1, 100000) // name, quantity, price (VND)
        };

            var paymentData = new PaymentData(
                orderCode,
                100000, // total amount
                "Order payment",
                items,
                "https://yourdomain.com/payment-cancel",
                "https://yourdomain.com/payment-success"
            );

            var paymentLink = await _payOS.createPaymentLink(paymentData);
            return Redirect(paymentLink.checkoutUrl);
        }
    }
}
