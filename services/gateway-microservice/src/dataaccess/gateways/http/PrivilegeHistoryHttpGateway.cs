using core.domain;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.dataaccess.gateways;
using dataaccess.converters.http;
using dataaccess.dto.http;
using dataaccess.dto.http.PrivilegeHistory;

namespace dataaccess.gateways.http;

/// <summary>
/// HTTP Gateway implementation for PrivilegeHistory API within Bonus microservice
/// </summary>
public class PrivilegeHistoryHttpGateway : BaseHttpGateway, IPrivilegeHistoryGateway
{
    private const string ApiEndpoint = "/api/v1/privilege-histories";

    public PrivilegeHistoryHttpGateway(HttpClient httpClient, string baseUrl)
        : base(httpClient, baseUrl)
    {
    }

    public async Task<IEnumerable<PrivilegeHistory>> GetAllAsync()
    {
        var dtos = await GetAsync<IEnumerable<PrivilegeHistoryDTO>>(ApiEndpoint);
        return dtos?.Select(PrivilegeHistoryHttpConverter.ToDomain) ?? Enumerable.Empty<PrivilegeHistory>();
    }

    public async Task<PrivilegeHistory?> GetByIdAsync(int id)
    {
        try
        {
            var dto = await GetAsync<PrivilegeHistoryDTO>($"{ApiEndpoint}/{id}");
            return dto != null ? PrivilegeHistoryHttpConverter.ToDomain(dto) : null;
        }
        catch (GatewayEntityNotFoundException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<PrivilegeHistory>> GetAllAsync(PrivilegeHistoryFilter filter)
    {
        var queryString = BuildFilterQueryString(filter);
        var dtos = await GetAsync<IEnumerable<PrivilegeHistoryDTO>>($"{ApiEndpoint}{queryString}");
        return dtos?.Select(PrivilegeHistoryHttpConverter.ToDomain) ?? Enumerable.Empty<PrivilegeHistory>();
    }

    public async Task<PrivilegeHistory> CreateAsync(PrivilegeHistory privilegeHistory)
    {
        var dto = PrivilegeHistoryHttpConverter.ToCreateDTO(privilegeHistory);
        var createdDto = await PostAsync<CreatePrivilegeHistoryDTO>($"{ApiEndpoint}", dto);
        return PrivilegeHistoryHttpConverter.ToDomain(createdDto);
    }

    public async Task<PrivilegeHistory> UpdateAsync(PrivilegeHistory privilegeHistory)
    {
        var dto = PrivilegeHistoryHttpConverter.ToUpdateDTO(privilegeHistory);
        var updatedDto = await PutAsync<UpdatePrivilegeHistoryDTO>($"{ApiEndpoint}/{privilegeHistory.Id}", dto);
        return PrivilegeHistoryHttpConverter.ToDomain(updatedDto);
    }

    public async Task DeleteAsync(int id)
    {
        await DeleteAsync($"{ApiEndpoint}/{id}");
    }

    /// <summary>
    /// Gets all privilege history records for a specific privilege
    /// </summary>
    public async Task<IEnumerable<PrivilegeHistory>> GetByPrivilegeIdAsync(int privilegeId)
    {
        var dtos = await GetAsync<IEnumerable<PrivilegeHistoryDTO>>($"{ApiEndpoint}/privilege/{privilegeId}");
        return dtos?.Select(PrivilegeHistoryHttpConverter.ToDomain) ?? Enumerable.Empty<PrivilegeHistory>();
    }

    protected override Exception CreateNotFoundException(string entityName, int? entityId)
    {
        return entityId.HasValue
            ? new PrivilegeHistoryGatewayEntityNotFoundException($"{entityName} with id {entityId} not found")
            : new PrivilegeHistoryGatewayEntityNotFoundException($"{entityName} not found");
    }

    private string BuildFilterQueryString(PrivilegeHistoryFilter filter)
    {
        var parameters = new List<string>();

        if (filter.PrivilegeId.HasValue)
            parameters.Add($"privilegeId={filter.PrivilegeId.Value}");
        if (filter.TicketUid.HasValue)
            parameters.Add($"ticketUid={filter.TicketUid.Value}");
        if (filter.OperationType.HasValue)
            parameters.Add($"operationType={(int)filter.OperationType.Value}");
        if (filter.MinBalanceDiff.HasValue)
            parameters.Add($"minBalanceDiff={filter.MinBalanceDiff.Value}");
        if (filter.MaxBalanceDiff.HasValue)
            parameters.Add($"maxBalanceDiff={filter.MaxBalanceDiff.Value}");
        if (filter.DateTimeFrom.HasValue)
            parameters.Add($"dateFrom={filter.DateTimeFrom.Value:yyyy-MM-dd}");
        if (filter.DateTimeUntil.HasValue)
            parameters.Add($"dateTo={filter.DateTimeUntil.Value:yyyy-MM-dd}");

        return parameters.Any() ? "?" + string.Join("&", parameters) : string.Empty;
    }
}
