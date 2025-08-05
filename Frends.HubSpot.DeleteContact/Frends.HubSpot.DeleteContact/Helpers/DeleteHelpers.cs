using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.DeleteContact.Helpers;

/// <summary>
/// Helper method for deleting HubSpot contacts.
/// </summary>
internal static class DeleteHelpers
{
    /// <summary>
    /// Attempts to retrieve the email address of a HubSpot contact using their contact ID.
    /// </summary>
    /// <param name="client">The Http client configured with authorization headers.</param>
    /// <param name="baseUrl">Base Url for the API, typically https://api.hubapi.com.</param>
    /// <param name="contactId">The unique Id of the contact to look up.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>The email address of the contact if found, otherwise null.</returns>
    public static async Task<string> GetContactEmail(HttpClient client, string baseUrl, string contactId, CancellationToken cancellationToken)
    {
        var endpoint = $"{baseUrl.TrimEnd('/')}/crm/v3/objects/contacts/{contactId}?properties=email";
        var response = await client.GetAsync(endpoint, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseJson = JObject.Parse(responseContent);
            return responseJson["properties"]?["email"]?.ToString();
        }

        return null;
    }
}
