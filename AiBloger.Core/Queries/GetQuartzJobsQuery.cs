using AiBloger.Core.Mediator;

namespace AiBloger.Core.Queries;

public record GetQuartzJobsQuery() : IRequest<IReadOnlyList<QuartzJobInfo>>;

public record QuartzJobInfo(
    string Group,
    string Name,
    string? Description,
    DateTimeOffset? LastFireTimeUtc,
    DateTimeOffset? NextFireTimeUtc,
    IReadOnlyList<QuartzTriggerInfo> Triggers);

public record QuartzTriggerInfo(
    string Group,
    string Name,
    string Type,
    string State,
    DateTimeOffset? LastFireTimeUtc,
    DateTimeOffset? NextFireTimeUtc);


