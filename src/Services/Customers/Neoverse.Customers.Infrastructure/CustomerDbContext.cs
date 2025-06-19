using Microsoft.EntityFrameworkCore;
using Neoverse.Customers.Domain.Entities;
using Neoverse.SharedKernel.Entities;

namespace Neoverse.Customers.Infrastructure;

public class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Translation> Translations => Set<Translation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(b =>
        {
            b.HasQueryFilter(c => !c.IsDeleted);
            b.OwnsOne(c => c.Email, eo => eo.Property(e => e.Value).HasColumnName("Email"));
        });
    }
}
