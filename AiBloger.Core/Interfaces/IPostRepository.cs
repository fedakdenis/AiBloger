using AiBloger.Core.Entities;

namespace AiBloger.Core.Interfaces;

public interface IPostRepository
{
    Task<Post> AddAsync(Post post);
}
