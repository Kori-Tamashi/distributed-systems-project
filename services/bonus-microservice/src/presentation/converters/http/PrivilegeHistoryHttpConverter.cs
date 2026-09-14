using presentation.dto.http;
using presentation.dto.http.PrivilegeHistory;

namespace presentation.converters.http;

/// <summary>
/// Converter for PrivilegeHistory between domain and HTTP DTO representations
/// </summary>
public static class PrivilegeHistoryHttpConverter
{
    /// <summary>
    /// Converts domain PrivilegeHistory to PrivilegeHistoryDTO
    /// </summary>
    public static PrivilegeHistoryDTO ToDTO(core.domain.PrivilegeHistory privilegeHistory)
    {
        return new PrivilegeHistoryDTO
        {
            Id = privilegeHistory.Id,
            PrivilegeId = privilegeHistory.PrivilegeId,
            TicketUid = privilegeHistory.TicketUid,
            DateTime = privilegeHistory.DateTime,
            BalanceDiff = privilegeHistory.BalanceDiff,
            OperationType = (int)privilegeHistory.OperationType
        };
    }

    /// <summary>
    /// Converts PrivilegeHistoryDTO to domain PrivilegeHistory
    /// </summary>
    public static core.domain.PrivilegeHistory ToDomain(PrivilegeHistoryDTO dto)
    {
        return new core.domain.PrivilegeHistory
        {
            Id = dto.Id,
            PrivilegeId = dto.PrivilegeId,
            TicketUid = dto.TicketUid,
            DateTime = dto.DateTime,
            BalanceDiff = dto.BalanceDiff,
            OperationType = (core.enums.OperationType)dto.OperationType
        };
    }

    /// <summary>
    /// Converts list of domain PrivilegeHistories to list of PrivilegeHistoryDTOs
    /// </summary>
    public static List<PrivilegeHistoryDTO> ToDTO(List<core.domain.PrivilegeHistory> privilegeHistories)
    {
        return privilegeHistories.Select(ToDTO).ToList();
    }

    /// <summary>
    /// Converts list of PrivilegeHistoryDTOs to list of domain PrivilegeHistories
    /// </summary>
    public static List<core.domain.PrivilegeHistory> ToDomain(List<PrivilegeHistoryDTO> dtos)
    {
        return dtos.Select(ToDomain).ToList();
    }

    /// <summary>
    /// Converts domain PrivilegeHistory to CreatePrivilegeHistoryDTO
    /// </summary>
    public static CreatePrivilegeHistoryDTO ToCreateDTO(core.domain.PrivilegeHistory privilegeHistory)
    {
        return new CreatePrivilegeHistoryDTO
        {
            PrivilegeId = privilegeHistory.PrivilegeId,
            TicketUid = privilegeHistory.TicketUid,
            DateTime = privilegeHistory.DateTime,
            BalanceDiffNegative = privilegeHistory.BalanceDiff,
            OperationType = (int)privilegeHistory.OperationType
        };
    }

    /// <summary>
    /// Converts CreatePrivilegeHistoryDTO to domain PrivilegeHistory
    /// </summary>
    public static core.domain.PrivilegeHistory ToCreateDomain(CreatePrivilegeHistoryDTO dto)
    {
        return new core.domain.PrivilegeHistory
        {
            Id = 0,
            PrivilegeId = dto.PrivilegeId,
            TicketUid = dto.TicketUid,
            DateTime = dto.DateTime,
            BalanceDiff = dto.BalanceDiffNegative,
            OperationType = (core.enums.OperationType)dto.OperationType
        };
    }

    /// <summary>
    /// Converts domain PrivilegeHistory to UpdatePrivilegeHistoryDTO
    /// </summary>
    public static UpdatePrivilegeHistoryDTO ToUpdateDTO(core.domain.PrivilegeHistory privilegeHistory)
    {
        return new UpdatePrivilegeHistoryDTO
        {
            Id = privilegeHistory.Id,
            PrivilegeId = privilegeHistory.PrivilegeId,
            TicketUid = privilegeHistory.TicketUid,
            DateTime = privilegeHistory.DateTime,
            BalanceDiffNegative = privilegeHistory.BalanceDiff,
            OperationType = (int)privilegeHistory.OperationType
        };
    }

    /// <summary>
    /// Converts UpdatePrivilegeHistoryDTO to domain PrivilegeHistory
    /// Only updates non-null properties
    /// </summary>
    public static core.domain.PrivilegeHistory ToUpdateDomain(UpdatePrivilegeHistoryDTO dto, core.domain.PrivilegeHistory existingPrivilegeHistory)
    {
        if (dto.PrivilegeId.HasValue)
            existingPrivilegeHistory.PrivilegeId = dto.PrivilegeId.Value;
        if (dto.TicketUid.HasValue)
            existingPrivilegeHistory.TicketUid = dto.TicketUid.Value;
        if (dto.DateTime.HasValue)
            existingPrivilegeHistory.DateTime = dto.DateTime.Value;
        if (dto.BalanceDiffNegative.HasValue)
            existingPrivilegeHistory.BalanceDiff = dto.BalanceDiffNegative.Value;
        if (dto.OperationType.HasValue)
            existingPrivilegeHistory.OperationType = (core.enums.OperationType)dto.OperationType.Value;

        return existingPrivilegeHistory;
    }

    /// <summary>
    /// Converts domain PrivilegeHistory to UpdatePrivilegeHistoryDTO (without existing privilege history)
    /// </summary>
    public static UpdatePrivilegeHistoryDTO ToUpdateDTOWithoutMerge(core.domain.PrivilegeHistory privilegeHistory)
    {
        return new UpdatePrivilegeHistoryDTO
        {
            Id = privilegeHistory.Id,
            PrivilegeId = privilegeHistory.PrivilegeId,
            TicketUid = privilegeHistory.TicketUid,
            DateTime = privilegeHistory.DateTime,
            BalanceDiffNegative = privilegeHistory.BalanceDiff,
            OperationType = (int)privilegeHistory.OperationType
        };
    }

    /// <summary>
    /// Converts domain PrivilegeHistory to UpdatePrivilegeHistoryDTO with only changed properties
    /// </summary>
    public static UpdatePrivilegeHistoryDTO ToUpdateDTOPartial(core.domain.PrivilegeHistory privilegeHistory, params string[] changedProperties)
    {
        var dto = new UpdatePrivilegeHistoryDTO(privilegeHistory.Id);

        if (Array.Exists(changedProperties, p => p.Equals("PrivilegeId", StringComparison.OrdinalIgnoreCase)))
            dto.PrivilegeId = privilegeHistory.PrivilegeId;

        if (Array.Exists(changedProperties, p => p.Equals("TicketUid", StringComparison.OrdinalIgnoreCase)))
            dto.TicketUid = privilegeHistory.TicketUid;

        if (Array.Exists(changedProperties, p => p.Equals("DateTime", StringComparison.OrdinalIgnoreCase)))
            dto.DateTime = privilegeHistory.DateTime;

        if (Array.Exists(changedProperties, p => p.Equals("BalanceDiff", StringComparison.OrdinalIgnoreCase)))
            dto.BalanceDiffNegative = privilegeHistory.BalanceDiff;

        if (Array.Exists(changedProperties, p => p.Equals("OperationType", StringComparison.OrdinalIgnoreCase)))
            dto.OperationType = (int)privilegeHistory.OperationType;

        return dto;
    }

    /// <summary>
    /// Converts domain PrivilegeHistory to UpdatePrivilegeHistoryDTO with only changed properties (nullable check)
    /// </summary>
    public static UpdatePrivilegeHistoryDTO ToUpdateDTOPartialNullable(core.domain.PrivilegeHistory privilegeHistory, params bool[] propertyChanged)
    {
        var dto = new UpdatePrivilegeHistoryDTO(privilegeHistory.Id);

        if (propertyChanged.Length >= 1 && propertyChanged[0])
            dto.PrivilegeId = privilegeHistory.PrivilegeId;

        if (propertyChanged.Length >= 2 && propertyChanged[1])
            dto.TicketUid = privilegeHistory.TicketUid;

        if (propertyChanged.Length >= 3 && propertyChanged[2])
            dto.DateTime = privilegeHistory.DateTime;

        if (propertyChanged.Length >= 4 && propertyChanged[3])
            dto.BalanceDiffNegative = privilegeHistory.BalanceDiff;

        if (propertyChanged.Length >= 5 && propertyChanged[4])
            dto.OperationType = (int)privilegeHistory.OperationType;

        return dto;
    }
}
