using AiBloger.Core.Entities;
using AiBloger.Core.Interfaces;
using AiBloger.Infrastructure.Data;

namespace AiBloger.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly NewsDbContext _context;

    public PostRepository(NewsDbContext context)
    {
        _context = context;
    }

    public async Task<Post> AddAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }
}
