using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptsController : Controller
{
    private readonly Data.AppContext _context;

    public ReceiptsController(Data.AppContext context)
    {
        _context = context;
    }

    // GET: api/Receipts --> RF51
}
