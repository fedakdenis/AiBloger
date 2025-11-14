using AiBloger.Core.Commands;
using AiBloger.Core.Entities;
using AiBloger.Core.Mediator;
using Microsoft.AspNetCore.Mvc;
using AiBloger.Web.Models;
using AiBloger.Core.Interfaces;

namespace AiBloger.Web.Controllers
{
	public class PostController : Controller
	{
		private readonly IMediator _mediator;
		private readonly IModelCatalog _modelCatalog;

		public PostController(IMediator mediator, IModelCatalog modelCatalog)
		{
			_mediator = mediator;
			_modelCatalog = modelCatalog;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var models = await _modelCatalog.GetModelIdsAsync();
			var vm = new PostPreviewViewModel
			{
				AvailableModels = models,
				Model = models.FirstOrDefault() ?? "gpt-4.1"
			};
			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> Index(PostPreviewViewModel vm)
        {
			vm.AvailableModels = await _modelCatalog.GetModelIdsAsync();

			if (string.IsNullOrWhiteSpace(vm.Url) || string.IsNullOrWhiteSpace(vm.Model))
			{
				vm.Error = "Url and Model are required.";
				return View(vm);
			}

			PostInfo info = await _mediator.Send(new GeneratePostPreviewCommand(vm.Url, vm.Model));
			vm.Result = info;
			return View(vm);
		}
	}
}


