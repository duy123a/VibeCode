using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;
using VibeCode.Shared.Entities;
using VibeCode.Shared.Entities.Interfaces;

namespace VibeCode.Main.Data;

public class VibeCodeDbContext : DbContext
{
    public VibeCodeDbContext(DbContextOptions<VibeCodeDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Page> Pages { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;

    private readonly IHttpContextAccessor _httpContextAccessor;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(VibeCodeDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { builder });
            }
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder)
        where TEntity : class, ISoftDeletable
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInfo()
    {
        var now = DateTimeOffset.UtcNow;
        var currentUser = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditable auditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditable.CreatedAt = now;
                        auditable.CreatedBy = currentUser;
                        auditable.UpdatedAt = now;
                        auditable.UpdatedBy = currentUser;
                        break;
                    case EntityState.Modified:
                        auditable.UpdatedAt = now;
                        auditable.UpdatedBy = currentUser;
                        break;
                }
            }

            if (entry.Entity is ISoftDeletable deletable && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                deletable.IsDeleted = true;
                deletable.DeletedAt = now;
                deletable.DeletedBy = currentUser;
            }
        }
    }
}
