using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        public static async Task DeleteTestContact(string contactId, string apiKey, string baseUrl = "https://api.hubapi.com", bool hardDelete = true, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(contactId))
                return;

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("Api Key is required", nameof(apiKey));

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base Url is required", nameof(baseUrl));

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var endpoint = $"{baseUrl.TrimEnd('/')}/crm/v3/objects/contacts/{contactId}"
                        + (hardDelete ? "?hardDelete=true" : string.Empty);

            var response = await client.DeleteAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Failed to delete test contact {contactId}. " + $"Status: {response.StatusCode}. " + $"Error: {error}");
            }
        }
    }
}