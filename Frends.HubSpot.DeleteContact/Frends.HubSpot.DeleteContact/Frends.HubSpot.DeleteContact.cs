using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.DeleteContact.Definitions;
using Frends.HubSpot.DeleteContact.Helpers;
using Newtonsoft.Json;
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

            if (!long.TryParse(input.ContactId, out _))
                throw new Exception($"Contact ID should be a numeric value: '{input.ContactId}'.");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {connection.ApiKey}");

            HttpResponseMessage response;

            if (options.HardDelete)
            {
                var endpoint = $"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/contacts/gdpr-delete";
                var requestBody = new
                {
                    idProperty = "id",
                    objectId = input.ContactId,
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json");

                response = await client.PostAsync(endpoint, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var contactEmail = await DeleteHelpers.GetContactEmail(client, connection.BaseUrl, input.ContactId, cancellationToken);

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
                        return new Result(true);
                    }
                }
            }
            else
            {
                var endpoint = $"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/contacts/{input.ContactId}";
                response = await client.DeleteAsync(endpoint, cancellationToken);
            }

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                try
                {
                    var responseJson = JObject.Parse(responseContent);
                    var error = responseJson["message"]?.ToString() ?? responseContent;

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new Result(true);
                    }

                    throw new Exception($"HubSpot API error: {response.StatusCode} - {error}");
                }
                catch (JsonReaderException)
                {
                    throw new Exception($"HubSpot API error: {response.StatusCode} - {responseContent}");
                }
            }

            return new Result(true);
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}