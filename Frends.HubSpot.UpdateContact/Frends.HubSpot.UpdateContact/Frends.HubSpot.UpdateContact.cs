using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.UpdateContact.Definitions;
using Frends.HubSpot.UpdateContact.Helpers;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.UpdateContact;

/// <summary>
/// Task class for updating a HubSpot contact.
/// </summary>
public static class HubSpot
{
    /// <summary>
    /// Updates a contact in HubSpot.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-HubSpot-UpdateContact)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Object { bool Success, Error Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> UpdateContact(
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

            if (string.IsNullOrWhiteSpace(input.ContactId))
                throw new Exception("ContactId is required");

            if (string.IsNullOrWhiteSpace(input.UpdateData))
                throw new Exception("UpdateData is required");

            if (!long.TryParse(input.ContactId, out _))
                throw new Exception($"Contact ID should be a numeric value: '{input.ContactId}'.");

            JObject contactProperties;

            try
            {
                contactProperties = JObject.Parse(input.UpdateData);
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                throw new Exception("Invalid JSON format in ContactData", ex);
            }

            if (options.CheckIfExists && !await UpdateHelpers.ContactExists(input.ContactId, connection.ApiKey, connection.BaseUrl, cancellationToken))
            {
                return ErrorHandler.Handle(new Exception($"Contact with ID {input.ContactId} does not exist"), options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {connection.ApiKey}");

            var payload = new JObject
            {
                ["properties"] = contactProperties,
            };

            var content = new StringContent(payload.ToString(), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PatchAsync($"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/contacts/{input.ContactId}", content, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = $"HubSpot API error: {response.StatusCode}";

                try
                {
                    var responseJson = JObject.Parse(responseContent);
                    var errorDetail = responseJson["message"]?.ToString();
                    message += errorDetail != null ? $" - {errorDetail}" : $" - {responseContent}";
                }
                catch
                {
                    message += $" - {responseContent}";
                }

                return ErrorHandler.Handle(new Exception(message), options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
            }

            return new Result(true);
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}
