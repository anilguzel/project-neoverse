using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Neoverse.SharedKernel.Attributes;
using System.Reflection;

namespace Neoverse.ApiBase.Filters;

public class DataPrivacyFilter : IAsyncResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.ContainsKey("X-Show-Private") &&
            context.Result is ObjectResult objectResult && objectResult.Value is not null)
        {
            Sanitize(objectResult.Value);
        }
        return next();
    }

    private static void Sanitize(object obj)
    {
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var attr = prop.GetCustomAttribute<DataPrivacyLevelAttribute>();
            if (attr?.Level == DataPrivacyLevel.High)
            {
                if (prop.CanWrite)
                {
                    prop.SetValue(obj, null);
                }
            }
        }
    }
}
