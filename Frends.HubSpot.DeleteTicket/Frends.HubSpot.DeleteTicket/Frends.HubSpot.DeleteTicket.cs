using Frends.HubSpot.DeleteTicket.Definitions;
using Frends.HubSpot.DeleteTicket.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.HubSpot.DeleteTicket;

/// <summary>
/// Task Class for HubSpot operations.
/// </summary>
public static class HubSpot
{
    /// <summary>
    /// Frends task for deleting a ticket in HubSpot.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-HubSpot-DeleteTicket)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Object { bool Success, string Id, Error Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> DeleteTicket(
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
            if (string.IsNullOrWhiteSpace(input.TicketId))
                throw new Exception("TicketId is required");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {connection.ApiKey}");

            var response = await client.DeleteAsync($"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/tickets/{input.TicketId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var responseJson = JObject.Parse(responseContent);
                var error = responseJson["message"]?.ToString() ?? "Unknown error";
                return ErrorHandler.Handle(new Exception($"HubSpot API error: {response.StatusCode} - {error}"), options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
            }

            return new Result { Success = true, Id = input.TicketId };
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}
