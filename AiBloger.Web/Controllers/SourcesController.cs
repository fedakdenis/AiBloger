using AiBloger.Core.Commands;
using AiBloger.Core.Entities;
using AiBloger.Core.Mediator;
using AiBloger.Core.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AiBloger.Web.Controllers
{
    public class SourcesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SourcesController> _logger;

        public SourcesController(IMediator mediator, ILogger<SourcesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var sources = await _mediator.Send(new GetSourcesQuery());
                return View(sources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sources");
                ViewBag.Error = "Error loading sources";
                return View(new List<Source>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(string jsonInput)
        {
            try
            {
                // Get current sources for display
                var sources = await _mediator.Send(new GetSourcesQuery());

                if (string.IsNullOrWhiteSpace(jsonInput))
                {
                    ViewBag.Error = "JSON input cannot be empty";
                    return View(sources);
                }

                // Parse JSON input
                var newSources = JsonSerializer.Deserialize<List<Source>>(jsonInput, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (newSources == null || !newSources.Any())
                {
                    ViewBag.Error = "Invalid JSON or empty sources list";
                    return View(sources);
                }

                // Add sources via mediator command
                var addedCount = await _mediator.Send(new AddSourcesBatchCommand(newSources));
                ViewBag.Success = $"Successfully added {addedCount} source(s)";

                // Reload sources
                sources = await _mediator.Send(new GetSourcesQuery());
                return View(sources);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error");
                ViewBag.Error = $"JSON parsing error: {ex.Message}";
                var sources = await _mediator.Send(new GetSourcesQuery());
                return View(sources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding sources");
                ViewBag.Error = $"Error adding sources: {ex.Message}";
                var sources = await _mediator.Send(new GetSourcesQuery());
                return View(sources);
            }
        }
    }
}

