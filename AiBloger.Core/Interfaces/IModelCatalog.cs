namespace AiBloger.Core.Interfaces;

public interface IModelCatalog
{
    Task<IReadOnlyList<string>> GetModelIdsAsync(CancellationToken cancellationToken = default);
}


