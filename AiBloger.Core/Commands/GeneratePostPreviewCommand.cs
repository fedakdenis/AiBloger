using AiBloger.Core.Entities;
using AiBloger.Core.Mediator;

namespace AiBloger.Core.Commands;

public record GeneratePostPreviewCommand(string Url, string Model) : IRequest<PostInfo>;


