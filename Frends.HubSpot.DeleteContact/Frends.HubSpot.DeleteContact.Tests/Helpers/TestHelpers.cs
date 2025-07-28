using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.DeleteContact.Tests.Helpers
{
    /// <summary>
    /// Helpers method for testing. Creates a contact.
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
    }
}