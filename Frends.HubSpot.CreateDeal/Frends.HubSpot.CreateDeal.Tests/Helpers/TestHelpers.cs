using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.CreateDeal.Tests.Helpers;

/// <summary>
/// Helper methods for managing test deals in HubSpot.
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Deletes a test deal from HubSpot.
    /// </summary>
    /// <param name="dealId">The unique dealId of the deal to delete.</param>
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
    /// <param name="dealId">The unique dealId of the deal to retrieve.</param>
    /// <param name="apiKey">HubSpot Private App access token.</param>
    /// <param name="baseUrl">Base Url for HubSpot Api.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Task representing the asynchronous operation with the contact data.</returns>
    public static async Task<string> GetTestDeal(string dealId, string apiKey, string baseUrl, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var endpoint = $"{baseUrl.TrimEnd('/')}/crm/v3/objects/deals/{dealId}?archived=false&associations=contacts";
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

    /// <summary>
    /// Creates a contact in HubSpot used for testing.
    /// </summary>
    /// <param name="apiKey">HubSpot Private App access token.</param>
    /// <param name="baseUrl">Base Url for the Api.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Contact Id used for testing functionality.</returns>
    public static async Task<string> CreateTestContact(string apiKey, string baseUrl, CancellationToken cancellationToken)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var contactProperties = JObject.Parse(
            $@"{{
                ""email"": ""testContact{timestamp}@example.com"",
                ""firstname"": ""Test"",
                ""lastname"": ""User"",
                ""phone"": ""+1234567890""
            }}");

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var payload = new JObject
        {
            ["properties"] = contactProperties,
        };

        var content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{baseUrl.TrimEnd('/')}/crm/v3/objects/contacts", content, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var responseJson = JObject.Parse(responseContent);

        return responseJson["id"]?.ToString();
    }

    /// <summary>
    /// Deletes a test contact from HubSpot.
    /// </summary>
    /// <param name="contactId">The unique Id of the contact to delete.</param>
    /// <param name="apiKey">HubSpot Private App access token.</param>
    /// <param name="baseUrl">Base Url for HubSpot Api.</param>
    /// <param name="hardDelete">If true, permanently deletes the contact (cannot be recovered).</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task DeleteTestContact(string contactId, string apiKey, string baseUrl, bool hardDelete, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        HttpResponseMessage response;

        if (hardDelete)
        {
            var endpoint = $"{baseUrl.TrimEnd('/')}/crm/v3/objects/contacts/gdpr-delete";
            var requestBody = new
            {
                idProperty = "id",
                objectId = contactId,
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json");

            response = await client.PostAsync(endpoint, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var contactEmail = await GetContactEmail(client, baseUrl, contactId, cancellationToken);

                if (!string.IsNullOrWhiteSpace(contactEmail))
                {
                    requestBody = new
                    {
                        idProperty = "email",
                        objectId = contactEmail,
                    };

                    content = new StringContent(
                        JsonConvert.SerializeObject(requestBody),
                        Encoding.UTF8,
                        "application/json");

                    response = await client.PostAsync(endpoint, content, cancellationToken);
                }
                else
                {
                    return;
                }
            }
        }
        else
        {
            var endpoint = $"{baseUrl.TrimEnd('/')}/crm/v3/objects/contacts/{contactId}";
            response = await client.DeleteAsync(endpoint, cancellationToken);
        }
    }

    /// <summary>
    /// Attempts to retrieve the email address of a HubSpot contact using their contact ID.
    /// </summary>
    /// <param name="client">The Http client configured with authorization headers.</param>
    /// <param name="baseUrl">Base Url for the API, typically https://api.hubapi.com.</param>
    /// <param name="contactId">The unique Id of the contact to look up.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>The email address of the contact if found, otherwise null.</returns>
    private static async Task<string> GetContactEmail(HttpClient client, string baseUrl, string contactId, CancellationToken cancellationToken)
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