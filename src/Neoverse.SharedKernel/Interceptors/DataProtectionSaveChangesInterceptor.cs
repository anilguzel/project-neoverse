using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Neoverse.SharedKernel.Configuration;
using Neoverse.SharedKernel.Attributes;
using Neoverse.SharedKernel.Entities;

namespace Neoverse.SharedKernel.Interceptors;

public class DataProtectionSaveChangesInterceptor : SaveChangesInterceptor, IMaterializationInterceptor
{
    private readonly byte[] _key;
    private static readonly byte[] Iv = new byte[16];

    public DataProtectionSaveChangesInterceptor(IOptions<DataProtectionSettings> options)
    {
        var keyString = options.Value.Key ?? "NeoverseEncryptionKey";
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(keyString));
    }
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is null) return base.SavingChanges(eventData, result);

        foreach (var entry in eventData.Context.ChangeTracker.Entries<BaseEntity>())
        {
            foreach (var prop in entry.Entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.PropertyType != typeof(string)) continue;
                var value = prop.GetValue(entry.Entity) as string;
                if (value is null) continue;

                if (prop.GetCustomAttribute<EncryptedAttribute>() != null)
                {
                    var encrypted = Encrypt(value);
                    prop.SetValue(entry.Entity, encrypted);
                }
                else if (prop.GetCustomAttribute<HashedAttribute>() != null)
                {
                    prop.SetValue(entry.Entity, Hash(value));
                }
            }
        }

        return base.SavingChanges(eventData, result);
    }

    public object InitializedInstance(MaterializationInterceptionData materializationData, object entity)
    {
        foreach (var prop in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.PropertyType != typeof(string)) continue;
            var value = prop.GetValue(entity) as string;
            if (value is null) continue;

            if (prop.GetCustomAttribute<EncryptedAttribute>() != null)
            {
                try
                {
                    var decrypted = Decrypt(value);
                    prop.SetValue(entity, decrypted);
                }
                catch
                {
                    // ignore if decryption fails
                }
            }
            else if (prop.GetCustomAttribute<HashedAttribute>() != null)
            {
                if (!Regex.IsMatch(value, "^[0-9a-fA-F]{64}$"))
                {
                    throw new InvalidOperationException($"Invalid hash value for {prop.Name}");
                }
            }
        }

        return entity;
    }

    private static string Hash(string value)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
        var hashed = BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        return hashed;
    }

    private string Encrypt(string plain)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = Iv;
        using var encryptor = aes.CreateEncryptor();
        var data = Encoding.UTF8.GetBytes(plain);
        var cipher = encryptor.TransformFinalBlock(data, 0, data.Length);
        return Convert.ToBase64String(cipher);
    }

    private string Decrypt(string cipher)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = Iv;
        using var decryptor = aes.CreateDecryptor();
        var data = Convert.FromBase64String(cipher);
        var plain = decryptor.TransformFinalBlock(data, 0, data.Length);
        return Encoding.UTF8.GetString(plain);
    }
}
