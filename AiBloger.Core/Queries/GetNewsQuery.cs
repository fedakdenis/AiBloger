using AiBloger.Core.Entities;
using AiBloger.Core.Mediator;

namespace AiBloger.Core.Queries;

public record GetNewsQuery(TimeSpan? Interval) : IRequest<IReadOnlyList<NewsItem>>;


