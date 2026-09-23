using dataaccess.dto.http;
using dataaccess.dto.http.Privilege;

namespace dataaccess.converters.http;

/// <summary>
/// Converter for Privilege between domain and HTTP DTO representations
/// </summary>
public static class PrivilegeHttpConverter
{
    /// <summary>
    /// Converts domain Privilege to PrivilegeDTO
    /// </summary>
    public static PrivilegeDTO ToDTO(core.domain.Privilege privilege)
    {
        return new PrivilegeDTO
        {
            Id = privilege.Id,
            Username = privilege.Username,
            Balance = privilege.Balance,
            Status = (int)privilege.Status
        };
    }

    /// <summary>
    /// Converts PrivilegeDTO to domain Privilege
    /// </summary>
    public static core.domain.Privilege ToDomain(PrivilegeDTO dto)
    {
        return new core.domain.Privilege
        {
            Id = dto.Id,
            Username = dto.Username,
            Balance = dto.Balance,
            Status = (core.enums.PrivilegeStatus)dto.Status
        };
    }

    /// <summary>
    /// Converts CreatePrivilegeDTO to domain Privilege
    /// </summary>
    public static core.domain.Privilege ToDomain(CreatePrivilegeDTO dto)
    {
        return new core.domain.Privilege
        {
            Id = dto.Id,
            Username = dto.Username,
            Balance = dto.Balance,
            Status = core.enums.PrivilegeStatus.BRONZE
        };
    }

    /// <summary>
    /// Converts UpdatePrivilegeDTO to domain Privilege
    /// </summary>
    public static core.domain.Privilege ToDomain(UpdatePrivilegeDTO dto)
    {
        return new core.domain.Privilege
        {
            Id = dto.Id,
            Username = dto.Username ?? string.Empty,
            Balance = dto.Balance ?? 0,
            Status = dto.Status.HasValue ? (core.enums.PrivilegeStatus)dto.Status.Value : core.enums.PrivilegeStatus.BRONZE
        };
    }

    /// <summary>
    /// Converts list of domain Privileges to list of PrivilegeDTOs
    /// </summary>
    public static List<PrivilegeDTO> ToDTO(List<core.domain.Privilege> privileges)
    {
        return privileges.Select(ToDTO).ToList();
    }

    /// <summary>
    /// Converts IEnumerable of domain Privileges to IEnumerable of PrivilegeDTOs
    /// </summary>
    public static IEnumerable<PrivilegeDTO> ToDTO(IEnumerable<core.domain.Privilege> privileges)
    {
        return privileges.Select(ToDTO);
    }
}
