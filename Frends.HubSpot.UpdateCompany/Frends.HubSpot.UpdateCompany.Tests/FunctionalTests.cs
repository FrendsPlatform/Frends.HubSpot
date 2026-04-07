using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.UpdateCompany.Definitions;
using Frends.HubSpot.UpdateCompany.Tests.Helpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.HubSpot.UpdateCompany.Tests;

[TestFixture]
public class FunctionalTests : TestBase
{
    private readonly string baseUrl = "https://api.hubapi.com";
    private readonly string dealId = "494473663705";
    private readonly string contactId = "335401013465";
    private string companyId;
    private Connection connection;
    private Input input;
    private Options options;

    [SetUp]
    public async Task Setup()
    {
        connection = new Connection
        {
            BaseUrl = baseUrl,
            ApiKey = ApiKey,
        };
        options = new Options
        {
            ThrowErrorOnFailure = true,
        };

        var createResult = await TestHelpers.CreateTestCompany(ApiKey, baseUrl, CancellationToken.None);
        companyId = createResult;

        input = new Input
        {
            Id = companyId,
            CompanyData = @"{
                ""name"": ""Updated Company"",
                ""domain"": ""updatedcompany.com"",
                ""phone"": ""987654321""
            }",
        };
    }

    [TearDown]
    public async Task Cleanup()
    {
        if (companyId != null)
        {
            var deleted = await TestHelpers.DeleteTestCompany(companyId, ApiKey, baseUrl, CancellationToken.None);
            if (!deleted)
                Console.WriteLine($"Warning: company {companyId} was not deleted - it may not have existed.");
            companyId = null;
        }
    }

    [Test]
    public async Task UpdateCompany_SuccessTest()
    {
        var result = await HubSpot.UpdateCompany(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Id, Is.EqualTo(companyId));

        var companyData = JObject.Parse(await TestHelpers.GetTestCompany(companyId, ApiKey, baseUrl, CancellationToken.None));
        Assert.That(companyData["id"]?.ToString(), Is.EqualTo(companyId));
        Assert.That(companyData["properties"]?["name"]?.ToString(), Is.EqualTo("Updated Company"));
        Assert.That(companyData["properties"]?["domain"]?.ToString(), Is.EqualTo("updatedcompany.com"));
    }

    [Test]
    public async Task UpdateCompany_WithPhone_SetsCorrectly()
    {
        var result = await HubSpot.UpdateCompany(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);

        var companyData = JObject.Parse(await TestHelpers.GetTestCompany(companyId, ApiKey, baseUrl, CancellationToken.None));
        Assert.That(companyData["properties"]?["phone"]?.ToString(), Is.EqualTo("987654321"));
    }

    [Test]
    public async Task UpdateCompany_WithAssociateWithContactId_AssociatesCorrectly()
    {
        options.AssociateWithContactId = contactId;

        var result = await HubSpot.UpdateCompany(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);

        var associations = JObject.Parse(await TestHelpers.GetTestCompanyAssociations(companyId, "contacts", ApiKey, baseUrl, CancellationToken.None));
        var associatedIds = associations["results"]?.Select(r => r["toObjectId"]?.ToString());
        Assert.That(associatedIds, Does.Contain(contactId));
    }

    [Test]
    public async Task UpdateCompany_WithAssociateWithDealId_AssociatesCorrectly()
    {
        options.AssociateWithDealId = dealId;

        var result = await HubSpot.UpdateCompany(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);

        var associations = JObject.Parse(await TestHelpers.GetTestCompanyAssociations(companyId, "deals", ApiKey, baseUrl, CancellationToken.None));
        var associatedIds = associations["results"]?.Select(r => r["toObjectId"]?.ToString());
        Assert.That(associatedIds, Does.Contain(dealId));
    }

    [Test]
    public async Task UpdateCompany_WithAssociateWithTicketId_AssociatesCorrectly()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
        var payload = new JObject
        {
            ["properties"] = JObject.Parse(@"{
                ""subject"": ""Test Ticket For Company Association"",
                ""hs_pipeline"": ""0"",
                ""hs_pipeline_stage"": ""1""
            }"),
        };
        var content = new StringContent(payload.ToString(), System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{baseUrl}/crm/v3/objects/tickets", content, CancellationToken.None);
        response.EnsureSuccessStatusCode();
        var ticketId = JObject.Parse(await response.Content.ReadAsStringAsync())["id"]?.ToString();

        try
        {
            options.AssociateWithTicketId = ticketId;

            var result = await HubSpot.UpdateCompany(input, connection, options, CancellationToken.None);
            Assert.That(result.Success, Is.True);

            var associations = JObject.Parse(await TestHelpers.GetTestCompanyAssociations(companyId, "tickets", ApiKey, baseUrl, CancellationToken.None));
            var associatedIds = associations["results"]?.Select(r => r["toObjectId"]?.ToString());
            Assert.That(associatedIds, Does.Contain(ticketId));
        }
        finally
        {
            await TestHelpers.DeleteTestTicket(ticketId, ApiKey, baseUrl, CancellationToken.None);
        }
    }

    [Test]
    public async Task UpdateCompany_InvalidId_ThrowsError()
    {
        input.Id = "000000000000";
        options.ThrowErrorOnFailure = false;

        var result = await HubSpot.UpdateCompany(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
    }
}
