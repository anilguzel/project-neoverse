using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Neoverse.SharedKernel.Attributes;
using System.Reflection;

namespace Neoverse.ApiBase.Filters;

public class ResponseMaskingFilter : IAsyncResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is not null)
        {
            MaskObject(objectResult.Value, new HashSet<object>(ReferenceEqualityComparer.Instance));
        }

        return next();
    }

    private static void MaskObject(object obj, HashSet<object> visited)
    {
        if (obj == null || obj is string || obj.GetType().IsPrimitive)
            return;

        if (!visited.Add(obj))
            return;

        var type = obj.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResult<>))
        {
            var dataProp = type.GetProperty("Data");
            var dataValue = dataProp?.GetValue(obj);
            if (dataValue != null)
            {
                MaskObject(dataValue, visited);
            }
            return;
        }

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            var value = prop.GetValue(obj);

            if (prop.GetCustomAttribute<MaskOnResponseAttribute>() != null && value is string str)
            {
                prop.SetValue(obj, new string('*', str.Length));
            }
            else if (value != null && !(value is string) && !prop.PropertyType.IsPrimitive)
            {
                if (value is IEnumerable enumerable && !(value is string))
                {
                    foreach (var item in enumerable)
                    {
                        MaskObject(item, visited);
                    }
                }
                else
                {
                    MaskObject(value, visited);
                }
            }
        }
    }
}

