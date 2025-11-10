using Microsoft.AspNetCore.Mvc;

namespace AiBloger.Web.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}


