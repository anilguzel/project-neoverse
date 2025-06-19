using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Neoverse.Customers.Domain.Entities;
using Neoverse.Customers.Infrastructure;
using Neoverse.SharedKernel.Interceptors;
using Xunit;

public class MultiLanguageIntegrationTests
{
    private static CustomerDbContext CreateContext(MultiLanguageSaveChangesInterceptor interceptor)
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new CustomerDbContext(options);
    }

    [Fact]
    public async Task SavesTranslation_WhenCustomerCreated()
    {
        var interceptor = new MultiLanguageSaveChangesInterceptor();
        using var ctx = CreateContext(interceptor);

        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        try
        {
            var customer = new Customer("John", new Email("john@example.com"));
            await ctx.Customers.AddAsync(customer);
            await ctx.SaveChangesAsync();

            var translation = await ctx.Translations.FirstOrDefaultAsync(t =>
                t.EntityId == customer.Id &&
                t.PropertyName == nameof(Customer.Name) &&
                t.Language == "en");

            Assert.NotNull(translation);
            Assert.Equal("John", translation!.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public async Task UpdatesTranslation_WhenCultureChanges()
    {
        var interceptor = new MultiLanguageSaveChangesInterceptor();
        using var ctx = CreateContext(interceptor);

        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        var customer = new Customer("John", new Email("john@example.com"));
        await ctx.Customers.AddAsync(customer);
        await ctx.SaveChangesAsync();

        // Fransız versiyon geliyor
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
        customer.UpdateName("Jean");
        await ctx.SaveChangesAsync();

        try
        {
            var fr = await ctx.Translations.FirstOrDefaultAsync(t =>
                t.EntityId == customer.Id &&
                t.PropertyName == nameof(Customer.Name) &&
                t.Language == "fr");

            Assert.NotNull(fr);
            Assert.Equal("Jean", fr!.Value);

            var localized = await GetLocalizedCustomer(ctx, customer.Id, "fr");
            Assert.NotNull(localized);
            Assert.Equal("Jean", localized!.Name);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private static async Task<Customer?> GetLocalizedCustomer(CustomerDbContext ctx, Guid id, string lang)
    {
        var customer = await ctx.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return null;

        var translation = await ctx.Translations.FirstOrDefaultAsync(t =>
            t.EntityId == id && t.PropertyName == nameof(Customer.Name) && t.Language == lang);

        if (translation != null)
        {
            var nameProp = typeof(Customer).GetProperty(nameof(Customer.Name),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            nameProp?.SetValue(customer, translation.Value);
        }

        return customer;
    }
}
