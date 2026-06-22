using Microsoft.AspNetCore.Mvc;

namespace Cardápio.Controllers
{
    public class DemoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
