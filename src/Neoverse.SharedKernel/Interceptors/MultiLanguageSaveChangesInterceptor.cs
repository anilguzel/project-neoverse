using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;
using System.Globalization;
using Neoverse.SharedKernel.Attributes;
using Neoverse.SharedKernel.Entities;

namespace Neoverse.SharedKernel.Interceptors;

public class MultiLanguageSaveChangesInterceptor : SaveChangesInterceptor
{
    private static void HandleTranslations(DbContext context)
    {
        var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        var entries = context.ChangeTracker.Entries<BaseEntity>().ToList();

        foreach (var entry in entries)
        {
            var properties = entry.Entity.GetType().GetProperties().ToList();

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<MultiLanguageAttribute>() != null &&
                    property.GetValue(entry.Entity) is string text)
                {
                    var translations = context.Set<Translation>();
                    var entityType = entry.Entity.GetType().Name;

                    var existing = translations.FirstOrDefault(t =>
                        t.EntityId == entry.Entity.Id &&
                        t.PropertyName == property.Name &&
                        t.Language == culture);

                    if (existing is null)
                    {
                        translations.Add(new Translation
                        {
                            EntityId = entry.Entity.Id,
                            EntityType = entityType,
                            PropertyName = property.Name,
                            Language = culture,
                            Value = text
                        });
                    }
                    else
                    {
                        existing.Value = text;
                    }
                }
            }
        }
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            HandleTranslations(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            HandleTranslations(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
