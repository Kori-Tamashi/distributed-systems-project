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
    public static PrivilegeHistoryDTO ToDTO(core.domain.PrivilegeHistory history)
    {
        if (history == null) return null!;
        
        return new PrivilegeHistoryDTO
        {
            Id = history.Id,
            PrivilegeId = history.PrivilegeId,
            TicketUid = history.TicketUid,
            DateTime = history.DateTime,
            BalanceDiff = history.BalanceDiff,
            OperationType = (int)history.OperationType,
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
    public static List<PrivilegeHistoryDTO> ToDTO(List<core.domain.PrivilegeHistory> histories)
    {
        return histories.Select(ToDTO).ToList();
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
    public static CreatePrivilegeHistoryDTO ToCreateDTO(core.domain.PrivilegeHistory history)
    {
        return new CreatePrivilegeHistoryDTO
        {
            PrivilegeId = history.PrivilegeId,
            TicketUid = history.TicketUid,
            DateTime = history.DateTime,
            BalanceDiff = history.BalanceDiff,
            OperationType = (int)history.OperationType
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
            BalanceDiff = dto.BalanceDiff,
            OperationType = (core.enums.OperationType)dto.OperationType,
        };
    }

    /// <summary>
    /// Converts domain PrivilegeHistory to UpdatePrivilegeHistoryDTO
    /// </summary>
    public static UpdatePrivilegeHistoryDTO ToUpdateDTO(core.domain.PrivilegeHistory history)
    {
        return new UpdatePrivilegeHistoryDTO
        {
            PrivilegeId = history.PrivilegeId,
            TicketUid = history.TicketUid,
            DateTime = history.DateTime,
            BalanceDiff = history.BalanceDiff,
            OperationType = (int)history.OperationType
        };
    }

    /// <summary>
    /// Converts UpdatePrivilegeHistoryDTO to domain PrivilegeHistory
    /// Only updates non-null properties
    /// </summary>
    public static core.domain.PrivilegeHistory ToUpdateDomain(UpdatePrivilegeHistoryDTO dto, core.domain.PrivilegeHistory existingHistory)
    {
        if (dto == null || existingHistory == null) return existingHistory;
        
        existingHistory.PrivilegeId = dto.PrivilegeId ?? existingHistory.PrivilegeId;
        existingHistory.TicketUid = dto.TicketUid ?? existingHistory.TicketUid;
        existingHistory.DateTime = dto.DateTime ?? existingHistory.DateTime;
        existingHistory.BalanceDiff = dto.BalanceDiff ?? existingHistory.BalanceDiff;
        existingHistory.OperationType = dto.OperationType.HasValue ? (core.enums.OperationType)dto.OperationType.Value : existingHistory.OperationType;

        return existingHistory;
    }
}
