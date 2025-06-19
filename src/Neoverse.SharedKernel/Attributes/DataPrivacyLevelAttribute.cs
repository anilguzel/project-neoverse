namespace Neoverse.SharedKernel.Attributes;

public enum DataPrivacyLevel
{
    Low,
    Medium,
    High
}

[AttributeUsage(AttributeTargets.Property)]
public class DataPrivacyLevelAttribute : Attribute
{
    public DataPrivacyLevel Level { get; }
    public DataPrivacyLevelAttribute(DataPrivacyLevel level) => Level = level;
}
