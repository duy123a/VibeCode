using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VibeCode.IdentityServer.Data;
using VibeCode.Shared.Interfaces;

namespace VibeCode.IdentityServer.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AuthDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(AuthDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(object id)
    {
        return await DbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.Where(predicate).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await DbSet.AddRangeAsync(entities);
    }

    public async Task UpdateAsync(T entity)
    {
        DbSet.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(T entity)
    {
        DbSet.Remove(entity);
        await Task.CompletedTask;
    }
}
