using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Frends.HubSpot.CreateContact.Tests.Helpers
{
    /// <summary>
    /// Helper methods for managing test contacts in HubSpot.
    /// </summary>
    internal static class TestHelpers
    {
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
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("API Key is required");

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new Exception("Base URL is required");

            if (string.IsNullOrWhiteSpace(contactId))
                throw new Exception("ContactId is required");

            if (!long.TryParse(contactId, out _))
                throw new Exception($"Contact ID should be a numeric value: '{contactId}'.");

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
            }
            else
            {
                var endpoint = $"{baseUrl.TrimEnd('/')}/crm/v3/objects/contacts/{contactId}";
                response = await client.DeleteAsync(endpoint, cancellationToken);
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Failed to delete test contact {contactId}. " + $"Status: {response.StatusCode}. " + $"Error: {error}");
            }
        }
    }
}