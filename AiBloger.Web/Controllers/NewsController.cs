using AiBloger.Core.Mediator;
using AiBloger.Core.Entities;
using AiBloger.Core.Queries;
using Microsoft.AspNetCore.Mvc;

namespace AiBloger.Web.Controllers
{
	public class NewsController : Controller
	{
		private readonly IMediator _mediator;

		public NewsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<IActionResult> Index(int? hours = 24)
		{
			TimeSpan? interval = null;
			if (hours.HasValue && hours.Value > 0)
			{
				interval = TimeSpan.FromHours(hours.Value);
			}

			IReadOnlyList<NewsItem> items = await _mediator.Send(new GetNewsQuery(interval));
			return View(items);
		}
	}
}


