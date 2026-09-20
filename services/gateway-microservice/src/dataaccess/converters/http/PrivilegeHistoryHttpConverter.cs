using dataaccess.dto.http;
using dataaccess.dto.http.PrivilegeHistory;

namespace dataaccess.converters.http;

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
    /// Converts CreatePrivilegeHistoryDTO to domain PrivilegeHistory
    /// </summary>
    public static core.domain.PrivilegeHistory ToDomain(CreatePrivilegeHistoryDTO dto)
    {
        return new core.domain.PrivilegeHistory
        {
            PrivilegeId = dto.PrivilegeId,
            TicketUid = dto.TicketUid,
            DateTime = dto.DateTime,
            BalanceDiff = dto.BalanceDiffNegative,
            OperationType = (core.enums.OperationType)dto.OperationType
        };
    }

    /// <summary>
    /// Converts UpdatePrivilegeHistoryDTO to domain PrivilegeHistory
    /// </summary>
    public static core.domain.PrivilegeHistory ToDomain(UpdatePrivilegeHistoryDTO dto)
    {
        var result = new core.domain.PrivilegeHistory
        {
            Id = dto.Id,
            PrivilegeId = dto.PrivilegeId ?? 0,
            TicketUid = dto.TicketUid ?? Guid.Empty,
            DateTime = dto.DateTime ?? DateTime.MinValue,
            BalanceDiff = dto.BalanceDiffNegative ?? 0
        };

        if (dto.OperationType.HasValue)
        {
            result.OperationType = (core.enums.OperationType)dto.OperationType.Value;
        }

        return result;
    }

    /// <summary>
    /// Converts list of domain PrivilegeHistories to list of PrivilegeHistoryDTOs
    /// </summary>
    public static List<PrivilegeHistoryDTO> ToDTO(List<core.domain.PrivilegeHistory> privilegeHistories)
    {
        return privilegeHistories.Select(ToDTO).ToList();
    }

    /// <summary>
    /// Converts IEnumerable of domain PrivilegeHistories to IEnumerable of PrivilegeHistoryDTOs
    /// </summary>
    public static IEnumerable<PrivilegeHistoryDTO> ToDTO(IEnumerable<core.domain.PrivilegeHistory> privilegeHistories)
    {
        return privilegeHistories.Select(ToDTO);
    }
}
