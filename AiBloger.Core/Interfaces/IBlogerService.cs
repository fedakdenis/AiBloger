using AiBloger.Core.Entities;

namespace AiBloger.Core.Interfaces;

public interface IBlogerService
{
    Task<bool> PublishPostAsync(Post post, CancellationToken cancellationToken = default);
}
