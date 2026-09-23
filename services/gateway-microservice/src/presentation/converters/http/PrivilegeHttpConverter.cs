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
        if (privilege == null) return null!;
        
        return new PrivilegeDTO
        {
            Id = privilege.Id,
            Username = privilege.Username,
            Status = (int)privilege.Status,
            Balance = privilege.Balance,
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
            Balance = dto.Balance,
        };
    }

    /// <summary>
    /// Converts domain Privilege to UpdatePrivilegeDTO
    /// </summary>
    public static UpdatePrivilegeDTO ToUpdateDTO(core.domain.Privilege privilege)
    {
        return new UpdatePrivilegeDTO
        {
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
        if (dto == null || existingPrivilege == null) return existingPrivilege;
        
        existingPrivilege.Username = dto.Username ?? existingPrivilege.Username;
        existingPrivilege.Status = dto.Status.HasValue ? (core.enums.PrivilegeStatus)dto.Status.Value : existingPrivilege.Status;
        existingPrivilege.Balance = dto.Balance ?? existingPrivilege.Balance;

        return existingPrivilege;
    }
}
