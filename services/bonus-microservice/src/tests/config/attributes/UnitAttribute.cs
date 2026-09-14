namespace tests.config.attributes;

/// <summary>
/// Marker attribute for unit tests
/// Used for test categorization and filtering
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class UnitAttribute : Attribute
{
}

/// <summary>
/// Marker attribute for integration tests
/// Used for test categorization and filtering
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class IntegrationAttribute : Attribute
{
}
