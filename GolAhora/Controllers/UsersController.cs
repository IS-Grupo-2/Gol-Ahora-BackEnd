using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
