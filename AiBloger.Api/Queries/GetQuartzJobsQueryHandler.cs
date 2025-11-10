using AiBloger.Core.Mediator;
using AiBloger.Core.Queries;
using Quartz;
using Quartz.Impl.Matchers;

namespace AiBloger.Api.Queries;

public sealed class GetQuartzJobsQueryHandler : IRequestHandler<GetQuartzJobsQuery, IReadOnlyList<QuartzJobInfo>>
{
    private readonly ISchedulerFactory _schedulerFactory;

    public GetQuartzJobsQueryHandler(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public async Task<IReadOnlyList<QuartzJobInfo>> Handle(GetQuartzJobsQuery request, CancellationToken cancellationToken)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var jobGroupNames = await scheduler.GetJobGroupNames(cancellationToken);
        var result = new List<QuartzJobInfo>();

        foreach (var group in jobGroupNames)
        {
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group), cancellationToken);
            foreach (var jobKey in jobKeys)
            {
                var detail = await scheduler.GetJobDetail(jobKey, cancellationToken);
                var triggers = await scheduler.GetTriggersOfJob(jobKey, cancellationToken);

                var triggerInfos = new List<QuartzTriggerInfo>();
                foreach (var trigger in triggers)
                {
                    var state = await scheduler.GetTriggerState(trigger.Key, cancellationToken);
                    triggerInfos.Add(new QuartzTriggerInfo(
                        trigger.Key.Group,
                        trigger.Key.Name,
                        trigger.GetType().Name,
                        state.ToString(),
                        trigger.GetPreviousFireTimeUtc(),
                        trigger.GetNextFireTimeUtc()
                    ));
                }

                DateTimeOffset? lastFire = triggerInfos
                    .Select(t => t.LastFireTimeUtc)
                    .Where(t => t.HasValue)
                    .DefaultIfEmpty(null)
                    .Max();

                DateTimeOffset? nextFire = triggerInfos
                    .Select(t => t.NextFireTimeUtc)
                    .Where(t => t.HasValue)
                    .DefaultIfEmpty(null)
                    .Min();

                result.Add(new QuartzJobInfo(
                    jobKey.Group,
                    jobKey.Name,
                    detail?.Description,
                    lastFire,
                    nextFire,
                    triggerInfos));
            }
        }

        return result;
    }
}


