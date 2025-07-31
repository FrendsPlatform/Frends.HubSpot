using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.GetContacts.Definitions;
using Frends.HubSpot.GetContacts.Helpers;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.GetContacts;

/// <summary>
/// Task class.
/// </summary>
public static class HubSpot
{
    /// <summary>
    /// Retrieves contacts from HubSpot.
    /// [Documentation](https://developers.hubspot.com/docs/api/crm/contacts)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>Object { bool Success, JToken Contacts, bool HasMore, string NextPageCursor, Error Error }</returns>
    public static async Task<Result> GetContacts(
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

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {connection.ApiKey}");

            var url = $"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/contacts";
            var queryParams = System.Web.HttpUtility.ParseQueryString(string.Empty);

            if (!string.IsNullOrWhiteSpace(input.FilterQuery))
                queryParams.Add("filter", input.FilterQuery);

            if (input.Properties != null && input.Properties.Length > 0)
                queryParams.Add("properties", string.Join(",", input.Properties));

            if (input.Limit > 0)
                queryParams.Add("limit", input.Limit.ToString());

            if (!string.IsNullOrWhiteSpace(input.After))
                queryParams.Add("after", input.After);

            if (queryParams.Count > 0)
                url += "?" + queryParams.ToString();

            var response = await client.GetAsync(url, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseJson = JObject.Parse(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = responseJson["message"]?.ToString() ?? "Unknown error";
                throw new Exception($"HubSpot API error: {response.StatusCode} - {error}");
            }

            return new Result(true, responseJson["results"], !string.IsNullOrEmpty(responseJson["paging"]?["next"]?["after"]?.ToString()), responseJson["paging"]?["next"]?["after"]?.ToString(), null);
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}