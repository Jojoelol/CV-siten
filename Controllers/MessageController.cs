using Microsoft.AspNetCore.Mvc;

namespace CV_siten.Controllers
{
    public class MessageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Message()
        {
            // Berätta specifikt för controllern att den ska titta i Account-mappen
            return View("~/Views/Account/Message.cshtml");
        }
    }
}
