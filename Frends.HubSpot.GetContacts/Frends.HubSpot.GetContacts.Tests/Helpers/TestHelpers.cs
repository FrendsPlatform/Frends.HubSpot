using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.HubSpot.GetContacts.Tests.Helpers
{
    /// <summary>
    /// Helper methods for testing.
    /// </summary>
    internal static class TestHelpers
    {
        /// <summary>
        /// Creates a contact in HubSpot used for testing.
        /// </summary>
        /// <param name="apiKey">HubSpot Private App access token.</param>
        /// <param name="baseUrl">Base URL for the API.</param>
        /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
        /// <returns>Contact Id used for testing delete functionality.</returns>
        public static async Task<string> CreateTestContact(string apiKey, string baseUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API Key is required");

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL is required");

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

            var content = new StringContent(payload.ToString(), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl.TrimEnd('/')}/crm/v3/objects/contacts", content, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseJson = JObject.Parse(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = responseJson["message"]?.ToString() ?? "Unknown error";
                throw new Exception($"HubSpot API error: {response.StatusCode} - {error}");
            }

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