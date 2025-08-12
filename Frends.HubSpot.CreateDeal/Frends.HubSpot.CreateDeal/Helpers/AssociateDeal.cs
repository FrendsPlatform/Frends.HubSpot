using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        try
        {
            var response = await client.PutAsync(
                $"{baseUrl.TrimEnd('/')}/crm/v3/objects/deals/{dealId}/associations/contacts/{contactId}/3",
                new StringContent("{}", Encoding.UTF8, "application/json"),
                cancellationToken);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
            {
                return;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Failed to associate deal with contact: {response.StatusCode} - {responseContent}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Association failed: {ex.Message}", ex);
        }
    }
}
