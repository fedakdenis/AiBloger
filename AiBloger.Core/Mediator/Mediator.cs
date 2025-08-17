using System.Reflection;

namespace AiBloger.Core.Mediator;

public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetService(handlerType);
        if (handler is null)
        {
            throw new InvalidOperationException($"Handler for {request.GetType().Name} not registered");
        }

        // Invoke Handle via reflection
        var method = handlerType.GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance);
        if (method == null)
        {
            throw new MissingMethodException(handlerType.FullName, "Handle");
        }

        var task = (Task)method.Invoke(handler, new object[] { request, cancellationToken })!;
        return AwaitTyped<TResponse>(task);
    }

    private static async Task<TResponse> AwaitTyped<TResponse>(Task task)
    {
        await task.ConfigureAwait(false);
        var resultProperty = task.GetType().GetProperty("Result");
        if (resultProperty == null)
        {
            // For Task (non-generic) this should not happen because we always expect Task<T>
            return default!;
        }
        return (TResponse)resultProperty.GetValue(task)!;
    }
}


