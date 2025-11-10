using AiBloger.Core.Mediator;
using AiBloger.Core.Queries;
using AiBloger.Web.Services;

namespace AiBloger.Web.Handlers
{
	public sealed class GetQuartzJobsFromApiHandler : IRequestHandler<GetQuartzJobsQuery, IReadOnlyList<QuartzJobInfo>>
	{
		private readonly IQuartzSchedulerApi _api;

		public GetQuartzJobsFromApiHandler(IQuartzSchedulerApi api)
		{
			_api = api;
		}

		public async Task<IReadOnlyList<QuartzJobInfo>> Handle(GetQuartzJobsQuery request, CancellationToken cancellationToken)
		{
			return await _api.GetJobsAsync(cancellationToken);
		}
	}
}


