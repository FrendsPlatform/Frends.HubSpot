using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.HubSpot.CreateDeal.Tests.Helpers;

/// <summary>
/// Helper methods for managing test deals in HubSpot.
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Deletes a test deal from HubSpot.
    /// </summary>
    /// <param name="dealId">The unique Id of the deal to delete.</param>
    /// <param name="apiKey">HubSpot Private App access token.</param>
    /// <param name="baseUrl">Base Url for HubSpot Api.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task DeleteTestDeal(string dealId, string apiKey, string baseUrl, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        HttpResponseMessage response;

        var endpoint = $"{baseUrl.TrimEnd('/')}/crm/v3/objects/deals/{dealId}";
        response = await client.DeleteAsync(endpoint, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Failed to delete deal {dealId}. Status: {response.StatusCode}. Error: {errorContent}");
        }
    }

    /// <summary>
    /// Retrieves a deal from HubSpot.
    /// </summary>
    /// <param name="dealId">The unique Id of the deal to retrieve.</param>
    /// <param name="apiKey">HubSpot Private App access token.</param>
    /// <param name="baseUrl">Base Url for HubSpot Api.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Task representing the asynchronous operation with the contact data.</returns>
    public static async Task<string> GetTestDeal(string dealId, string apiKey, string baseUrl, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var endpoint = $"{baseUrl.TrimEnd('/')}/crm/v3/objects/deals/{dealId}?archived=false";
        var response = await client.GetAsync(endpoint, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Failed to get deal {dealId}. Status: {response.StatusCode}. Error: {errorContent}");
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}