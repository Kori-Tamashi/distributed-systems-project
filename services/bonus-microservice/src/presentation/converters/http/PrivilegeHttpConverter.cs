using presentation.dto.http;
using presentation.dto.http.Privilege;

namespace presentation.converters.http;

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
            Status = (int)privilege.Status,
            Balance = privilege.Balance
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
            Status = (core.enums.PrivilegeStatus)dto.Status,
            Balance = dto.Balance
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
    /// Converts list of PrivilegeDTOs to list of domain Privileges
    /// </summary>
    public static List<core.domain.Privilege> ToDomain(List<PrivilegeDTO> dtos)
    {
        return dtos.Select(ToDomain).ToList();
    }

    /// <summary>
    /// Converts domain Privilege to CreatePrivilegeDTO
    /// </summary>
    public static CreatePrivilegeDTO ToCreateDTO(core.domain.Privilege privilege)
    {
        return new CreatePrivilegeDTO
        {
            Username = privilege.Username,
            Status = (int)privilege.Status,
            Balance = privilege.Balance
        };
    }

    /// <summary>
    /// Converts CreatePrivilegeDTO to domain Privilege
    /// </summary>
    public static core.domain.Privilege ToCreateDomain(CreatePrivilegeDTO dto)
    {
        return new core.domain.Privilege
        {
            Id = 0,
            Username = dto.Username,
            Status = (core.enums.PrivilegeStatus)dto.Status,
            Balance = dto.Balance
        };
    }

    /// <summary>
    /// Converts domain Privilege to UpdatePrivilegeDTO
    /// </summary>
    public static UpdatePrivilegeDTO ToUpdateDTO(core.domain.Privilege privilege)
    {
        return new UpdatePrivilegeDTO
        {
            Id = privilege.Id,
            Username = privilege.Username,
            Status = (int)privilege.Status,
            Balance = privilege.Balance
        };
    }

    /// <summary>
    /// Converts UpdatePrivilegeDTO to domain Privilege
    /// Only updates non-null properties
    /// </summary>
    public static core.domain.Privilege ToUpdateDomain(UpdatePrivilegeDTO dto, core.domain.Privilege existingPrivilege)
    {
        if (dto.Username != null)
            existingPrivilege.Username = dto.Username;
        if (dto.Status.HasValue)
            existingPrivilege.Status = (core.enums.PrivilegeStatus)dto.Status.Value;
        if (dto.Balance.HasValue)
            existingPrivilege.Balance = dto.Balance.Value;

        return existingPrivilege;
    }

    /// <summary>
    /// Converts domain Privilege to UpdatePrivilegeDTO (without existing privilege)
    /// </summary>
    public static UpdatePrivilegeDTO ToUpdateDTOWithoutMerge(core.domain.Privilege privilege)
    {
        return new UpdatePrivilegeDTO
        {
            Id = privilege.Id,
            Username = privilege.Username,
            Status = (int)privilege.Status,
            Balance = privilege.Balance
        };
    }

    /// <summary>
    /// Converts domain Privilege to UpdatePrivilegeDTO with only changed properties
    /// </summary>
    public static UpdatePrivilegeDTO ToUpdateDTOPartial(core.domain.Privilege privilege, params string[] changedProperties)
    {
        var dto = new UpdatePrivilegeDTO(privilege.Id);

        if (Array.Exists(changedProperties, p => p.Equals("Username", StringComparison.OrdinalIgnoreCase)))
            dto.Username = privilege.Username;

        if (Array.Exists(changedProperties, p => p.Equals("Status", StringComparison.OrdinalIgnoreCase)))
            dto.Status = (int)privilege.Status;

        if (Array.Exists(changedProperties, p => p.Equals("Balance", StringComparison.OrdinalIgnoreCase)))
            dto.Balance = privilege.Balance;

        return dto;
    }

    /// <summary>
    /// Converts domain Privilege to UpdatePrivilegeDTO with only changed properties (nullable check)
    /// </summary>
    public static UpdatePrivilegeDTO ToUpdateDTOPartialNullable(core.domain.Privilege privilege, params bool[] propertyChanged)
    {
        var dto = new UpdatePrivilegeDTO(privilege.Id);

        if (propertyChanged.Length >= 1 && propertyChanged[0])
            dto.Username = privilege.Username;

        if (propertyChanged.Length >= 2 && propertyChanged[1])
            dto.Status = (int)privilege.Status;

        if (propertyChanged.Length >= 3 && propertyChanged[2])
            dto.Balance = privilege.Balance;

        return dto;
    }
}
