using MediatR;

namespace AiBloger.Core.Commands;

public record AddNewsFromSourceCommand(string Source, string Url) : IRequest<int>;
