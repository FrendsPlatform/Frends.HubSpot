using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.HubSpot.CreateCompany.Tests.Helpers;

public static class TestHelpers
{
    public static async Task<bool> DeleteTestTicket(
           string ticketId,
           string apiKey,
           string baseUrl,
           CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        var response = await client.DeleteAsync(
            $"{baseUrl.TrimEnd('/')}/crm/v3/objects/tickets/{ticketId}",
            cancellationToken);

        if (response.StatusCode != HttpStatusCode.NoContent)
            throw new Exception($"Delete test ticket failed with status code {response.StatusCode}");
        try
        {
            var ticketData = await GetTestTicket("1", apiKey, baseUrl, cancellationToken);
            return ticketData == null
                ? throw new Exception(
                    "Checking if ticket exists failed - expected any data data or Exception but got null")
                : false;
        }
        catch (Exception e)
        {
            return e.Message.Contains("404")
                ? false
                : throw new Exception($"Checking if ticket exists failed with message {e.Message}");
        }
    }

    public static async Task<string> GetTestTicket(
            string ticketId,
            string apiKey,
            string baseUrl,
            CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        var response = await client.GetAsync(
            $"{baseUrl.TrimEnd('/')}/crm/v3/objects/tickets/{ticketId}" +
            "?properties=subject,content,hs_ticket_priority,hs_pipeline,hs_pipeline_stage,hs_ticket_category,hubspot_owner_id",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

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

    public static async Task<string> GetTestCompanyAssociations(
        string companyId,
        string toObjectType,
        string apiKey,
        string baseUrl,
        CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        var response = await client.GetAsync(
            $"{baseUrl.TrimEnd('/')}/crm/v4/objects/companies/{companyId}/associations/{toObjectType}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
