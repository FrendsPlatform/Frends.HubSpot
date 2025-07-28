using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.DeleteContact.Definitions;
using Frends.HubSpot.DeleteContact.Helpers;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.DeleteContact;

/// <summary>
/// Task class for deleting HubSpot contacts.
/// </summary>
public static class HubSpot
{
    /// <summary>
    /// Deletes a contact in HubSpot.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-HubSpot-DeleteContact)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Object { bool Success, Error Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> DeleteContact(
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

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {connection.ApiKey}");

            var endpoint = $"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/contacts/{input.ContactId}" + (options.HardDelete ? "?hardDelete=true" : string.Empty);

            var response = await client.DeleteAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorMessage = JObject.Parse(responseContent)?["message"]?.ToString() ?? $"HTTP {(int)response.StatusCode} - {response.ReasonPhrase}";

                throw new Exception($"HubSpot API error: {errorMessage}");
            }

            return new Result(true);
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}
