using AiBloger.Core.Commands;
using AiBloger.Core.Entities;
using AiBloger.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace AiBloger.Core.Mediator;

public static class RegisterCommandHandlersExtensions
{
    public static IServiceCollection RegisterCommandHandlers(this IServiceCollection services)
    {
        return services
            .AddScoped<IRequestHandler<AddNewsFromSourceCommand, int>, AddNewsFromSourceCommandHandler>()
            .AddScoped<IRequestHandler<WriteNewPostCommand, PostInfo>, WriteNewPostCommandHandler>();
    }
}


