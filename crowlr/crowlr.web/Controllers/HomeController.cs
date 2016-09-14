using Microsoft.AspNetCore.Mvc;

namespace crowlr.web.Controllers
{
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("test")]
        public string Test()
        {
            return "Hello World!";
        }
    }
}
