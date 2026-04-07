using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.DeleteCompany.Definitions;
using Frends.HubSpot.DeleteCompany.Helpers;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.DeleteCompany;

/// <summary>
/// Task Class for HubSpot operations.
/// </summary>
public static class HubSpot
{
    /// <summary>
    /// Frends task for deleting a company in HubSpot.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-HubSpot-DeleteCompany)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Id, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> DeleteCompany(
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

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {connection.ApiKey}");

            var response = await client.DeleteAsync(
                $"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/companies/{input.Id}",
                cancellationToken);

            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var responseJson = JObject.Parse(responseContent);
                var error = responseJson["message"]?.ToString() ?? "Unknown error";
                throw new Exception($"HubSpot API error: {response.StatusCode} - {error}");
            }

            return new Result { Success = true, Id = input.Id };
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}
