using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.CreateDeal.Definitions;
using Frends.HubSpot.CreateDeal.Helpers;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.CreateDeal;

/// <summary>
/// Task class.
/// </summary>
public static class HubSpot
{
    /// <summary>
    /// Creates a deal in HubSpot.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-HubSpot-CreateContact)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Object { bool Success, string Id, Error Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> CreateDeal(
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

            if (string.IsNullOrWhiteSpace(input.DealData))
                throw new Exception("DealData is required");

            JObject dealProperties;

            try
            {
                dealProperties = JObject.Parse(input.DealData);
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                throw new Exception("Invalid JSON format in DealData", ex);
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {connection.ApiKey}");

            var payload = new JObject
            {
                ["properties"] = dealProperties,
            };

            var content = new StringContent(payload.ToString(), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/deals", content, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseJson = JObject.Parse(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = responseJson["message"]?.ToString() ?? "Unknown error";
                throw new Exception($"HubSpot API error: {response.StatusCode} - {error}");
            }

            var dealId = responseJson["id"]?.ToString();

            if (!string.IsNullOrWhiteSpace(options.AssociateWithContactData))
            {
                await AssociateDeal.AssociateDealWithContact(client, connection.BaseUrl, dealId, options.AssociateWithContactData, cancellationToken);
            }

            return new Result(true, dealId);
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}