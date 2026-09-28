using Xunit;

namespace tests.config.attributes;

/// <summary>
/// Custom attribute to mark unit tests
/// Helps categorize and filter tests
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class UnitAttribute : FactAttribute
{
    /// <summary>
    /// Creates a new instance of UnitAttribute
    /// </summary>
    public UnitAttribute() : base()
    {
    }
}

/// <summary>
/// Marker attribute for integration tests
/// Used for test categorization and filtering
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class IntegrationAttribute : Attribute
{
}
