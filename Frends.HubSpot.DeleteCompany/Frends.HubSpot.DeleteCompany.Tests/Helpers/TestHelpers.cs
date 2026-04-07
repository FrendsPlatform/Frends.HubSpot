using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.DeleteCompany.Tests.Helpers;

public static class TestHelpers
{
    public static async Task<bool> DeleteTestCompany(
        string companyId,
        string apiKey,
        string baseUrl,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        var response = await client.DeleteAsync(
            $"{baseUrl.TrimEnd('/')}/crm/v3/objects/companies/{companyId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();

        try
        {
            await GetTestCompany(companyId, apiKey, baseUrl, cancellationToken);
            throw new Exception($"Company {companyId} still exists after deletion");
        }
        catch (Exception e)
        {
            return e.Message.Contains("404")
                ? true
                : throw new Exception($"Checking if company exists failed with message {e.Message}");
        }
    }

    public static async Task<string> GetTestCompany(
        string companyId,
        string apiKey,
        string baseUrl,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        var response = await client.GetAsync(
            $"{baseUrl.TrimEnd('/')}/crm/v3/objects/companies/{companyId}" +
            "?properties=name,domain,phone",
            cancellationToken);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public static async Task<string> CreateTestCompany(
        string apiKey,
        string baseUrl,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        var payload = new JObject
        {
            ["properties"] = JObject.Parse(@"{
            ""name"": ""Test Company"",
            ""domain"": ""testcompany.com"",
            ""phone"": ""123456789""
        }"),
        };
        var content = new StringContent(payload.ToString(), System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync(
            $"{baseUrl.TrimEnd('/')}/crm/v3/objects/companies",
            content,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseJson = JObject.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        return responseJson["id"]!.ToString();
    }
}
