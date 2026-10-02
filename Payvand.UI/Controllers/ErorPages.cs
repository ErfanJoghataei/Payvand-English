using Microsoft.AspNetCore.Mvc;

namespace Payvand.UI.Controllers
{
    public class ErorPagesController : Controller
    {
        [HttpGet]
        public IActionResult Page404()
        {
            return View();
        }

        [HttpGet]
        public IActionResult NetworkProblem()
        {
            return View();
        }
    }
}
