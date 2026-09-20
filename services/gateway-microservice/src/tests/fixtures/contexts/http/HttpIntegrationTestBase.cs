using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace tests.fixtures.contexts.http;

/// <summary>
/// Base class for HTTP integration tests
/// Provides automatic skip when target API is unavailable
/// </summary>
public abstract class HttpIntegrationTestBase : IDisposable
{
    protected readonly HttpGatewayIntegrationTestContext TestContext;
    protected readonly ITestOutputHelper Output;
    protected bool IsApiAvailable;
    protected string ApiName;

    protected HttpIntegrationTestBase(HttpGatewayIntegrationTestContext context, ITestOutputHelper output, string apiName)
    {
        TestContext = context;
        Output = output;
        ApiName = apiName;
        
        // Check if API is available
        IsApiAvailable = CheckApiAvailability();
        
        if (!IsApiAvailable)
        {
        }
    }

    /// <summary>
    /// Check if the target API is available by making a simple GET request
    /// </summary>
    private bool CheckApiAvailability()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };
            client.BaseAddress = new Uri(TestContext.GetBaseUrl(ApiName.ToLower()));
            
            // Just try to connect - any response means API is available
            var response = client.GetAsync("/").Result;
            return true;
        }
        catch
        {
            // Connection failed - API not available
            return false;
        }
    }

    /// <summary>
    /// Skip test if API is not available
    /// </summary>
    protected bool SkipIfApiUnavailable()
    {
        if (!IsApiAvailable)
        {
            Output?.WriteLine($"[SKIP] {ApiName} API is not available - test skipped");
            return true; // Skip = true
        }
        return false; // Continue = false
    }

    /// <summary>
    /// Skip test if API is not available (with custom message)
    /// </summary>
    protected void SkipIfApiUnavailable(string reason)
    {
        if (!IsApiAvailable)
        {
            throw new InvalidOperationException($"[SKIPPED] {ApiName} API is not available");
        }
    }

    /// <summary>
    /// Check if API is unavailable (for conditional logic)
    /// </summary>
    protected bool IsApiUnavailable() => !IsApiAvailable;

    public void Dispose()
    {
        TestContext?.Dispose();
    }
}
