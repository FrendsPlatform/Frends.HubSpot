using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
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
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-HubSpot-GetContacts)
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

            HttpResponseMessage response;
            JObject responseJson;
            string url;

            if (!string.IsNullOrWhiteSpace(input.FilterQuery))
            {
                url = $"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/contacts/search";

                var parsed = FilterParser.ParseFilterQuery(input.FilterQuery);

                var filter = new JObject
                {
                    ["propertyName"] = parsed.PropertyName,
                    ["operator"] = parsed.Operator,
                };

                switch (parsed.Operator)
                {
                    case "IN":
                    case "NOT_IN":
                        filter["values"] = new JArray(parsed.Values);
                        break;
                    case "BETWEEN":
                        filter["value"] = parsed.Value;
                        filter["highValue"] = parsed.HighValue;
                        break;
                    case "HAS_PROPERTY":
                    case "NOT_HAS_PROPERTY":
                        break;
                    default:
                        filter["value"] = parsed.Value;
                        break;
                }

                var requestBody = new JObject
                {
                    ["filterGroups"] = new JArray
                    {
                        new JObject
                        {
                            ["filters"] = new JArray { filter },
                        },
                    },
                    ["properties"] = input.Properties != null ? new JArray(input.Properties) : [],
                    ["limit"] = input.Limit,
                };

                if (!string.IsNullOrWhiteSpace(input.After))
                    requestBody["after"] = input.After;

                var content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
                response = await client.PostAsync(url, content, cancellationToken);
            }
            else
            {
                url = $"{connection.BaseUrl.TrimEnd('/')}/crm/v3/objects/contacts";
                var queryParams = new Dictionary<string, string>
                {
                    ["archived"] = options.IncludeArchived.ToString().ToLower(),
                };

                if (input.Properties != null && input.Properties.Length > 0)
                    queryParams["properties"] = string.Join(",", input.Properties);

                if (input.Limit > 0)
                    queryParams["limit"] = input.Limit.ToString();

                if (!string.IsNullOrWhiteSpace(input.After))
                    queryParams["after"] = input.After;

                if (queryParams.Count > 0)
                    url += "?" + string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

                response = await client.GetAsync(url, cancellationToken);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            responseJson = JObject.Parse(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = responseJson["message"]?.ToString()
                    ?? responseJson["errors"]?.First?["message"]?.ToString()
                    ?? "Unknown error";

                return ErrorHandler.Handle(new Exception($"HubSpot API error: {response.StatusCode} - {error}"), options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
            }

            return new Result(true, responseJson["results"], !string.IsNullOrEmpty(responseJson["paging"]?["next"]?["after"]?.ToString()), responseJson["paging"]?["next"]?["after"]?.ToString(), null);
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}