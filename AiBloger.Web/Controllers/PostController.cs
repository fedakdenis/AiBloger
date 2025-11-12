using AiBloger.Core.Commands;
using AiBloger.Core.Entities;
using AiBloger.Core.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AiBloger.Web.Controllers
{
	public class PostController : Controller
	{
		private readonly IMediator _mediator;

		public PostController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Index(string url, string model)
		{
			if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(model))
			{
				ViewData["Error"] = "Url and Model are required.";
				return View();
			}

			PostInfo info = await _mediator.Send(new GeneratePostPreviewCommand(url, model));
			return View(info);
		}
	}
}


