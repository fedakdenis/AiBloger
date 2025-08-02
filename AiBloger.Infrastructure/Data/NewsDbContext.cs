using Microsoft.EntityFrameworkCore;
using AiBloger.Core.Entities;

namespace AiBloger.Infrastructure.Data;

public class NewsDbContext : DbContext
{
    public NewsDbContext(DbContextOptions<NewsDbContext> options) : base(options)
    {
    }

    public DbSet<NewsItem> NewsItems { get; set; }
    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration for NewsItem
        modelBuilder.Entity<NewsItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW()");
                
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW()");
                
            // Indexes for query optimization
            entity.HasIndex(e => e.Source);
            entity.HasIndex(e => e.PublishDate);
            entity.HasIndex(e => e.Url).IsUnique();
        });

        // Configuration for Post
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW()");
                
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW()");
                
            // Configure one-to-many relationship (NewsItem -> Posts)
            entity.HasOne(p => p.NewsItem)
                .WithMany(n => n.Posts)
                .HasForeignKey(p => p.NewsItemId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Indexes for query optimization
            entity.HasIndex(e => e.NewsItemId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        // Update timestamps for NewsItem
        var newsEntries = ChangeTracker.Entries<NewsItem>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in newsEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        // Update timestamps for Post
        var postEntries = ChangeTracker.Entries<Post>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in postEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
