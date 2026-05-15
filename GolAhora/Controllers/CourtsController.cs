using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    public class CourtsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
