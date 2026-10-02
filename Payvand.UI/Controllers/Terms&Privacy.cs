using Microsoft.AspNetCore.Mvc;

namespace Payvand.UI.Controllers
{
    public class Terms_PrivacyController : Controller
    {
        [HttpGet]
        public IActionResult Terms()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Privacy()

        {
            return View();
        }
    }
}
