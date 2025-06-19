using Microsoft.EntityFrameworkCore;
using Neoverse.SharedKernel.Entities;

namespace Neoverse.SharedKernel.Repositories.EF;

public class EfRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly DbContext Db;

    public EfRepository(DbContext db)
    {
        Db = db;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Db.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Db.Set<T>().AddAsync(entity, cancellationToken);
        await Db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        Db.Set<T>().Update(entity);
        await Db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        Db.Set<T>().Remove(entity);
        await Db.SaveChangesAsync(cancellationToken);
    }
}
