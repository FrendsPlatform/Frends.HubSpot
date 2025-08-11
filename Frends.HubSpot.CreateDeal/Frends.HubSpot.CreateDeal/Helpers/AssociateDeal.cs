using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.CreateDeal.Helpers;

/// <summary>
/// Association helper method
/// </summary>
public static class AssociateDeal
{
    /// <summary>
    /// Associates a created deal with a contact.
    /// </summary>
    /// <param name="client">Initialized HttpClient with authentication headers.</param>
    /// <param name="baseUrl">Base Url for the Api.</param>
    /// <param name="dealId">Id of the deal to associate.</param>
    /// <param name="contactId">Id of the contact to associate.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task AssociateDealWithContact(HttpClient client, string baseUrl, string dealId, string contactId, CancellationToken cancellationToken)
    {
        var associationPayload = new JObject
        {
            ["types"] = new JArray
            {
                new JObject
                {
                    ["associationCategory"] = "HUBSPOT_DEFINED",
                    ["associationTypeId"] = 3,
                },
            },
        };

        var content = new StringContent(associationPayload.ToString(), System.Text.Encoding.UTF8, "application/json");

        var response = await client.PutAsync(
            $"{baseUrl.TrimEnd('/')}/crm/v3/objects/deals/{dealId}/associations/contacts/{contactId}",
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseJson = JObject.Parse(responseContent);
            var error = responseJson["message"]?.ToString() ?? "Unknown error";
            throw new Exception($"Failed to associate deal with contact: {response.StatusCode} - {error}");
        }
    }
}
