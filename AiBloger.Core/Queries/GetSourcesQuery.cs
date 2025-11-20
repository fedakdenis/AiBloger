using AiBloger.Core.Entities;
using AiBloger.Core.Mediator;

namespace AiBloger.Core.Queries;

public record GetSourcesQuery() : IRequest<IReadOnlyList<Source>>;

