using Microsoft.EntityFrameworkCore;
using Neoverse.DocumentManagement.Domain.Entities;
using Neoverse.SharedKernel.Entities;

namespace Neoverse.DocumentManagement.Infrastructure;

public class DocumentDbContext : DbContext
{
    public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options) { }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Translation> Translations => Set<Translation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(b =>
        {
            b.HasQueryFilter(d => !d.IsDeleted);
        });
    }
}
