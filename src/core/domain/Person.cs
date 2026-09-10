namespace core.domain;

/// <summary>
/// Domain entity representing a Person
/// </summary>
public class Person
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Person's full name (required)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Person's age
    /// </summary>
    public int? Age { get; set; }

    /// <summary>
    /// Person's address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Person's workplace or job
    /// </summary>
    public string? Work { get; set; }
}
