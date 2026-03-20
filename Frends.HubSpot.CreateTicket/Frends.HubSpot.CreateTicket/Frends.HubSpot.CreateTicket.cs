using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.CreateTicket.Definitions;
using Frends.HubSpot.CreateTicket.Helpers;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.CreateTicket;

/// <summary>
/// Task Class for HubSpot operations.
/// </summary>
public static class HubSpot
{
    /// <summary>
    /// Frends task for creating a ticket in HubSpot.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-HubSpot-CreateTicket)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Object { bool Success, string Id, Error Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> CreateTicket(
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
            if (string.IsNullOrWhiteSpace(input.TicketData))
                throw new Exception("TicketData is required");

            JObject ticketProperties;
            try
            {
                ticketProperties = JObject.Parse(input.TicketData);
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                throw new Exception("Invalid JSON format in TicketData", ex);
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {connection.ApiKey}");

            var payload = new JObject
            {
                ["properties"] = ticketProperties,
            };

            var content = new StringContent(payload.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/tickets", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseJson = JObject.Parse(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = responseJson["message"]?.ToString() ?? "Unknown error";
                return ErrorHandler.Handle(new Exception($"HubSpot API error: {response.StatusCode} - {error}"), options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
            }

            var ticketId = responseJson["id"]?.ToString();

            if (!string.IsNullOrWhiteSpace(options.AssociateWithContactId))
                await AssociateTicket(client, connection.BaseUrl, ticketId, "contacts", options.AssociateWithContactId, cancellationToken);

            if (!string.IsNullOrWhiteSpace(options.AssociateWithDealId))
                await AssociateTicket(client, connection.BaseUrl, ticketId, "deals", options.AssociateWithDealId, cancellationToken);

            if (!string.IsNullOrWhiteSpace(options.AssociateWithCompanyId))
                await AssociateTicket(client, connection.BaseUrl, ticketId, "companies", options.AssociateWithCompanyId, cancellationToken);

            return new Result { Success = true, Id = ticketId };
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }

    private static async Task AssociateTicket(
    HttpClient client,
    string baseUrl,
    string ticketId,
    string toObjectType,
    string toObjectId,
    CancellationToken cancellationToken)
    {
        var url = $"{baseUrl.TrimEnd('/')}/crm/v4/objects/tickets/{ticketId}/associations/default/{toObjectType}/{toObjectId}";
        var response = await client.PutAsync(url, null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var json = JObject.Parse(body);
            var error = json["message"]?.ToString() ?? "Unknown error";
            throw new Exception($"Failed to associate ticket with {toObjectType}: {response.StatusCode} - {error}");
        }
    }
}
