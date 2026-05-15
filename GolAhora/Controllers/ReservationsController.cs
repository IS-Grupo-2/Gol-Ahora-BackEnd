using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    public class ReservationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
