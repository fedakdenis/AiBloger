using AiBloger.Core.Entities;

namespace AiBloger.Core.Interfaces;

public interface ISourceRepository
{
    Task<IEnumerable<Source>> GetAllAsync();
    Task<int> AddBatchAsync(IEnumerable<Source> sources);
}

