using AiBloger.Core.Entities;
using AiBloger.Core.Mediator;

namespace AiBloger.Core.Commands;

public record AddSourcesBatchCommand(IEnumerable<Source> Sources) : IRequest<int>;

