using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.CreateContact.Definitions;
using Frends.HubSpot.CreateContact.Helpers;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.CreateContact;

/// <summary>
/// Task class.
/// </summary>
public static class HubSpot
{
    /// <summary>
    /// Creates a contact in HubSpot.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-HubSpot-CreateContact)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Object { bool Success, string ContactId, Error Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> CreateContact(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(connection.ApiKey))
                throw new Exception("API Key is required");

            if (string.IsNullOrWhiteSpace(connection.BaseUrl))
                throw new Exception("Base URL is required");

            if (string.IsNullOrWhiteSpace(input.ContactData))
                throw new Exception("ContactData is required");

            JObject contactProperties;

            try
            {
                contactProperties = JObject.Parse(input.ContactData);
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                throw new Exception("Invalid JSON format in ContactData", ex);
            }

            if (options.ValidateEmail && contactProperties["email"] != null)
            {
                var email = contactProperties["email"].ToString();

                if (!ValidationHelper.IsValidEmail(email))
                    throw new Exception($"Invalid email format: {email}");
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {connection.ApiKey}");

            var payload = new JObject
            {
                ["properties"] = contactProperties,
            };

            var content = new StringContent(payload.ToString(), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/contacts", content, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseJson = JObject.Parse(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = responseJson["message"]?.ToString() ?? "Unknown error";
                throw new Exception($"HubSpot API error: {response.StatusCode} - {error}");
            }

            return new Result(true, responseJson["id"]?.ToString());
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}
