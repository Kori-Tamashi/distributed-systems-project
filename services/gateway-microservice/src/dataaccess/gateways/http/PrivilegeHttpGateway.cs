using core.domain;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.dataaccess.gateways;
using dataaccess.converters.http;
using dataaccess.dto.http;
using dataaccess.dto.http.Privilege;

namespace dataaccess.gateways.http;

/// <summary>
/// HTTP Gateway implementation for Privilege (Bonus) microservice
/// </summary>
public class PrivilegeHttpGateway : BaseHttpGateway, IPrivilegeGateway
{
    private const string ApiEndpoint = "/api/v1/privileges";

    public PrivilegeHttpGateway(HttpClient httpClient, string baseUrl)
        : base(httpClient, baseUrl)
    {
    }

    public async Task<IEnumerable<Privilege>> GetAllAsync()
    {
        var dtos = await GetAsync<IEnumerable<PrivilegeDTO>>(ApiEndpoint);
        return dtos?.Select(PrivilegeHttpConverter.ToDomain) ?? Enumerable.Empty<Privilege>();
    }

    public async Task<Privilege?> GetByIdAsync(int id)
    {
        try
        {
            var dto = await GetAsync<PrivilegeDTO>($"{ApiEndpoint}/{id}");
            return dto != null ? PrivilegeHttpConverter.ToDomain(dto) : null;
        }
        catch (GatewayEntityNotFoundException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Privilege>> GetAllAsync(PrivilegeFilter filter)
    {
        var queryString = BuildFilterQueryString(filter);
        var dtos = await GetAsync<IEnumerable<PrivilegeDTO>>($"{ApiEndpoint}{queryString}");
        return dtos?.Select(PrivilegeHttpConverter.ToDomain) ?? Enumerable.Empty<Privilege>();
    }

    public async Task<Privilege> CreateAsync(Privilege privilege)
    {
        var dto = PrivilegeHttpConverter.ToDTO(privilege);
        var createdDto = await PostAsync<CreatePrivilegeDTO>($"{ApiEndpoint}", dto);
        return PrivilegeHttpConverter.ToDomain(createdDto);
    }

    public async Task<Privilege> UpdateAsync(Privilege privilege)
    {
        var dto = PrivilegeHttpConverter.ToDTO(privilege);
        var updatedDto = await PutAsync<UpdatePrivilegeDTO>($"{ApiEndpoint}/{privilege.Id}", dto);
        return PrivilegeHttpConverter.ToDomain(updatedDto);
    }

    public async Task DeleteAsync(int id)
    {
        await DeleteAsync($"{ApiEndpoint}/{id}");
    }

    /// <summary>
    /// Credits bonus points to a privilege account
    /// </summary>
    public async Task<bool> CreditBalanceAsync(int privilegeId, int amount, string reason)
    {
        var dto = new CreditDebitBonusDTO { Amount = amount, Reason = reason };
        var result = await PostAsync<bool>($"{ApiEndpoint}/{privilegeId}/credit", dto);
        return result;
    }

    /// <summary>
    /// Debits bonus points from a privilege account
    /// </summary>
    public async Task<bool> DebitBalanceAsync(int privilegeId, int amount, string reason)
    {
        var dto = new CreditDebitBonusDTO { Amount = amount, Reason = reason };
        var result = await PostAsync<bool>($"{ApiEndpoint}/{privilegeId}/debit", dto);
        return result;
    }

    /// <summary>
    /// Gets maximum debit amount for a privilege account
    /// </summary>
    public async Task<int> GetMaxDebitAmountAsync(int privilegeId)
    {
        var result = await GetAsync<int>($"{ApiEndpoint}/{privilegeId}/maxdebit");
        return result;
    }

    protected override Exception CreateNotFoundException(string entityName, int? entityId)
    {
        return entityId.HasValue
            ? new PrivilegeGatewayEntityNotFoundException($"{entityName} with id {entityId} not found")
            : new PrivilegeGatewayEntityNotFoundException($"{entityName} not found");
    }

    private string BuildFilterQueryString(PrivilegeFilter filter)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrEmpty(filter.Username))
            parameters.Add($"username={Uri.EscapeDataString(filter.Username)}");
        if (filter.Status.HasValue)
            parameters.Add($"status={(int)filter.Status.Value}");
        if (filter.MinBalance.HasValue)
            parameters.Add($"minBalance={filter.MinBalance.Value}");
        if (filter.MaxBalance.HasValue)
            parameters.Add($"maxBalance={filter.MaxBalance.Value}");

        return parameters.Any() ? "?" + string.Join("&", parameters) : string.Empty;
    }
}
