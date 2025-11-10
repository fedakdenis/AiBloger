using AiBloger.Core.Queries;

namespace AiBloger.Web.Services
{
	public interface IQuartzSchedulerApi
	{
		Task<IReadOnlyList<QuartzJobInfo>> GetJobsAsync(CancellationToken cancellationToken = default);
	}
}


