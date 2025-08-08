using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.HubSpot.UpdateContact.Helpers;

/// <summary>
/// Update contact helper method.
/// </summary>
internal class UpdateHelpers
{
    /// <summary>
    /// Checks for contact's existence in HubSpot.
    /// </summary>
    /// <param name="contactId">The unique Id of the contact to retrieve.</param>
    /// <param name="apiKey">HubSpot Private App access token.</param>
    /// <param name="baseUrl">Base Url for HubSpot Api.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Boolean value, true if contact exists, false if not.</returns>
    public static async Task<bool> ContactExists(string contactId, string apiKey, string baseUrl, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var endpoint = $"{baseUrl.TrimEnd('/')}/crm/v3/objects/contacts/{contactId}?properties=id";
        var response = await client.GetAsync(endpoint, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"HubSpot API error: {response.StatusCode} - {errorContent}");
        }

        return true;
    }
}
