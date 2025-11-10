using AiBloger.Core.Mediator;
using AiBloger.Core.Queries;
using Microsoft.AspNetCore.Mvc;

namespace AiBloger.Web.Controllers
{
	public class JobsController : Controller
	{
		private readonly IMediator _mediator;

		public JobsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		public async Task<IActionResult> Index()
		{
			IReadOnlyList<QuartzJobInfo> jobs = await _mediator.Send(new GetQuartzJobsQuery());
			return View(jobs);
		}
	}
}


