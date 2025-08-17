using AiBloger.Core.Mediator;
using AiBloger.Core.Entities;

namespace AiBloger.Core.Commands;

public record WriteNewPostCommand() : IRequest<PostInfo>;
