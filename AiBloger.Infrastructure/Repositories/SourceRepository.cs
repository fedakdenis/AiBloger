using Microsoft.EntityFrameworkCore;
using AiBloger.Core.Entities;
using AiBloger.Core.Interfaces;
using AiBloger.Infrastructure.Data;

namespace AiBloger.Infrastructure.Repositories;

public class SourceRepository : ISourceRepository
{
    private readonly NewsDbContext _context;

    public SourceRepository(NewsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Source>> GetAllAsync()
    {
        return await _context.Sources
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<int> AddBatchAsync(IEnumerable<Source> sources)
    {
        if (!sources.Any())
            return 0;

        var names = sources.Select(s => s.Name).Distinct().ToList();
        var uris = sources.Select(s => s.Uri).Distinct().ToList();
        
        // Get existing sources by name or uri to avoid duplicates
        var existingSources = await _context.Sources
            .Where(s => names.Contains(s.Name) || uris.Contains(s.Uri))
            .ToListAsync();
        
        var existingNames = existingSources.Select(s => s.Name).ToHashSet();
        var existingUris = existingSources.Select(s => s.Uri).ToHashSet();
        
        // Filter out duplicates - only add sources that don't exist by name OR uri
        var newSources = sources
            .Where(s => !existingNames.Contains(s.Name) && !existingUris.Contains(s.Uri))
            .DistinctBy(s => s.Name)
            .ToList();

        if (!newSources.Any())
            return 0;

        await _context.Sources.AddRangeAsync(newSources);
        await _context.SaveChangesAsync();
        
        return newSources.Count;
    }
}

