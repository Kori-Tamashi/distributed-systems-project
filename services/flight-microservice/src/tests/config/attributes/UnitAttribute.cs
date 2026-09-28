namespace tests.config.attributes;

/// <summary>
/// Attribute to mark unit tests
/// Usage: [Unit] before test method or class
/// Note: For filtering, use: dotnet test --filter "FullyQualifiedName~Unit"
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public class UnitAttribute : Attribute
{
}
