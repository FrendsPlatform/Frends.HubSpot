using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.UpdateCompany.Definitions;
using Frends.HubSpot.UpdateCompany.Helpers;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.UpdateCompany;

/// <summary>
/// Task Class for HubSpot operations.
/// </summary>
public static class HubSpot
{
    /// <summary>
    /// Frends task for updating a company in HubSpot.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-HubSpot-UpdateCompany)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Object { bool Success, string Id, Error Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> UpdateCompany(
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

            if (string.IsNullOrWhiteSpace(input.Id))
                throw new Exception("Company Id is required");

            if (string.IsNullOrWhiteSpace(input.CompanyData))
                throw new Exception("CompanyData is required");

            JObject companyProperties;
            try
            {
                companyProperties = JObject.Parse(input.CompanyData);
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                throw new Exception("Invalid JSON format in CompanyData", ex);
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {connection.ApiKey}");

            var payload = new JObject
            {
                ["properties"] = companyProperties,
            };

            var content = new StringContent(payload.ToString(), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PatchAsync(
                $"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/companies/{input.Id}",
                content,
                cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseJson = JObject.Parse(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = responseJson["message"]?.ToString() ?? "Unknown error";
                throw new Exception($"HubSpot API error: {response.StatusCode} - {error}");
            }

            var companyId = input.Id;

            if (!string.IsNullOrWhiteSpace(options.AssociateWithContactId))
                await AssociateCompany(client, connection.BaseUrl, companyId, "contacts", options.AssociateWithContactId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(options.AssociateWithDealId))
                await AssociateCompany(client, connection.BaseUrl, companyId, "deals", options.AssociateWithDealId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(options.AssociateWithTicketId))
                await AssociateCompany(client, connection.BaseUrl, companyId, "tickets", options.AssociateWithTicketId, cancellationToken);

            return new Result { Success = true, Id = companyId };
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }

    private static async Task AssociateCompany(
        HttpClient client,
        string baseUrl,
        string companyId,
        string toObjectType,
        string toObjectId,
        CancellationToken cancellationToken)
    {
        var url = $"{baseUrl.TrimEnd('/')}/crm/v4/objects/companies/{companyId}/associations/default/{toObjectType}/{toObjectId}";
        var response = await client.PutAsync(url, null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var json = JObject.Parse(body);
            var error = json["message"]?.ToString() ?? "Unknown error";
            throw new Exception($"Failed to associate company with {toObjectType}: {response.StatusCode} - {error}");
        }
    }
}
