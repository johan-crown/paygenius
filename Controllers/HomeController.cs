using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PayGenius;
using PayGenius_Test.Models;
using System.Diagnostics;

namespace PayGenius_Test.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string token, string secret)
        {
            const string CARD_TOKEN = "cea26ac0-846a-45f6-a3c5-6005f44bc5bc";

            var url = Url.Action(nameof(CallBack), "Home", null, protocol: Request.Scheme, host: Request.Host.ToString());

            if (Request.Headers.TryGetValue("x-forwarded-host", out var host))
                url = Url.Action(nameof(CallBack), "Home", null, protocol: Request.Scheme, host: host.ToString());


            PayGeniusMainClient client = new PayGeniusMainClient(token, secret);

            var card = client.LookupCardByToken(new PayGenius.Requests.CardLookupByTokenRequest(CARD_TOKEN));

            var recpayResult = client.CreateRecurringPayment(new PayGenius.Requests.RecurringPaymentRequest(card.Card.Token, card.Card.Cvv, "INV001", 399, "", true, true, url));

            Console.WriteLine(JsonConvert.SerializeObject(recpayResult));

            TempData["tdsMethodContent"] = recpayResult.threeDSecureV2.tdsMethodContent;
            TempData["reference"] = recpayResult.reference;
            TempData["txId"] = recpayResult.threeDSecureV2.txId;
            TempData["transactionId"] = recpayResult.threeDSecureV2.transactionId;


            return RedirectToAction("Process");

            //var confirmPaymentResult = client.ConfirmPayment(new PayGenius.Requests.ConfirmPaymentRequest(recpayResult.reference, recpayResult.threeDSecure.paReq));

            //Console.WriteLine(confirmPaymentResult.message);
            //Console.ReadKey();

            return RedirectToAction("Index");
        }

        public IActionResult Process()
        {
            return View();
        }


        public IActionResult CallBack()
        {

            if (Request.Form != null)
                foreach (var item in Request.Form.Keys)
                    Console.WriteLine($"Key: {item} Value: {Request.Form[item]}");

            return Ok(true);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
