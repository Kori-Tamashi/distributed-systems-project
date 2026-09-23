namespace tests.config.attributes;

/// <summary>
/// Attribute to mark integration tests
/// Usage: [Integration] before test method or class
/// Note: For filtering, use: dotnet test --filter "FullyQualifiedName~Integration"
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public class IntegrationAttribute : Attribute
{
}
