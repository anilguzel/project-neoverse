using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neoverse.SharedKernel.Entities;

namespace Neoverse.SharedKernel.Localization;

public static class TranslationMerger
{
    public static void MergeTranslations<T>(T entity, IEnumerable<Translation> translations, string language)
    {
        if (entity is null || string.IsNullOrWhiteSpace(language)) return;
        foreach (var t in translations.Where(t => t.Language == language))
        {
            var prop = entity!.GetType().GetProperty(t.PropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite && prop.PropertyType == typeof(string))
            {
                prop.SetValue(entity, t.Value);
            }
        }
    }
}
