using System.Net.Http;
using System.Net.Http.Json;
using AiBloger.Core.Queries;
using Microsoft.Extensions.Logging;

namespace AiBloger.Web.Services
{
	public sealed class QuartzSchedulerApi : IQuartzSchedulerApi
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<QuartzSchedulerApi> _logger;

		public QuartzSchedulerApi(HttpClient httpClient, ILogger<QuartzSchedulerApi> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		public async Task<IReadOnlyList<QuartzJobInfo>> GetJobsAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				var jobs = await _httpClient.GetFromJsonAsync<IReadOnlyList<QuartzJobInfo>>("api/scheduler/jobs", cancellationToken);
				return jobs ?? Array.Empty<QuartzJobInfo>();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to fetch Quartz jobs");
				return Array.Empty<QuartzJobInfo>();
			}
		}
	}
}


