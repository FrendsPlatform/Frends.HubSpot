using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.UpdateTicket.Definitions;
using Frends.HubSpot.UpdateTicket.Tests.Helpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.HubSpot.UpdateTicket.Tests;

[TestFixture]
public class FunctionalTests : TestBase
{
    private readonly string baseUrl = "https://api.hubapi.com";
    private readonly string companyId = "175778808024";
    private readonly string dealId = "494473663705";
    private readonly string contactId = "335401013465";
    private string ticketId;
    private Connection connection;
    private Input input;
    private Options options;

    [SetUp]
    public async Task Setup()
    {
        connection = new Connection { BaseUrl = baseUrl, ApiKey = ApiKey };
        options = new Options { ThrowErrorOnFailure = true };

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
        var payload = new JObject
        {
            ["properties"] = JObject.Parse(@"{
            ""subject"": ""Original Subject"",
            ""hs_pipeline"": ""0"",
            ""hs_pipeline_stage"": ""1"",
            ""hs_ticket_priority"": ""LOW""
        }"),
        };
        var content = new StringContent(payload.ToString(), System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"{baseUrl}/crm/v3/objects/tickets", content, CancellationToken.None);
        response.EnsureSuccessStatusCode();
        var responseJson = JObject.Parse(await response.Content.ReadAsStringAsync());
        ticketId = responseJson["id"]?.ToString();

        input = new Input
        {
            TicketId = ticketId,
            TicketData = @"{ ""subject"": ""Updated Subject"", ""hs_ticket_priority"": ""HIGH"" }",
        };
    }

    [TearDown]
    public async Task Cleanup()
    {
        if (ticketId != null)
        {
            var deleted = await TestHelpers.DeleteTestTicket(ticketId, ApiKey, baseUrl, CancellationToken.None);
            if (!deleted)
                Console.WriteLine($"Warning: ticket {ticketId} was not deleted - it may not have existed.");
            ticketId = null;
        }
    }

    [Test]
    public async Task UpdateTicket_SuccessTest()
    {
        var result = await HubSpot.UpdateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Id, Is.EqualTo(ticketId));

        var ticketData = JObject.Parse(await TestHelpers.GetTestTicket(ticketId, ApiKey, baseUrl, CancellationToken.None));
        Assert.That(ticketData["properties"]?["subject"]?.ToString(), Is.EqualTo("Updated Subject"));
        Assert.That(ticketData["properties"]?["hs_ticket_priority"]?.ToString(), Is.EqualTo("HIGH"));
    }

    [Test]
    public async Task UpdateTicket_UpdatePriority_SetsCorrectly()
    {
        input.TicketData = @"{ ""hs_ticket_priority"": ""MEDIUM"" }";

        var result = await HubSpot.UpdateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);

        var ticketData = JObject.Parse(await TestHelpers.GetTestTicket(ticketId, ApiKey, baseUrl, CancellationToken.None));
        Assert.That(ticketData["properties"]?["hs_ticket_priority"]?.ToString(), Is.EqualTo("MEDIUM"));
    }

    [Test]
    public async Task UpdateTicket_UpdatePipelineStage_SetsCorrectly()
    {
        input.TicketData = @"{ ""hs_pipeline_stage"": ""2"" }";

        var result = await HubSpot.UpdateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);

        var ticketData = JObject.Parse(await TestHelpers.GetTestTicket(ticketId, ApiKey, baseUrl, CancellationToken.None));
        Assert.That(ticketData["properties"]?["hs_pipeline_stage"]?.ToString(), Is.EqualTo("2"));
    }

    [Test]
    public async Task UpdateTicket_UpdateContent_SetsCorrectly()
    {
        input.TicketData = @"{ ""content"": ""Updated content"" }";

        var result = await HubSpot.UpdateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);

        var ticketData = JObject.Parse(await TestHelpers.GetTestTicket(ticketId, ApiKey, baseUrl, CancellationToken.None));
        Assert.That(ticketData["properties"]?["content"]?.ToString(), Is.EqualTo("Updated content"));
    }

    [Test]
    public async Task UpdateTicket_WithAssociateWithContactId_AssociatesCorrectly()
    {
        options.AssociateWithContactId = contactId;

        var result = await HubSpot.UpdateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);

        var associations = JObject.Parse(await TestHelpers.GetTestTicketAssociations(ticketId, "contacts", ApiKey, baseUrl, CancellationToken.None));
        var associatedIds = associations["results"]?.Select(r => r["toObjectId"]?.ToString());
        Assert.That(associatedIds, Does.Contain(contactId));
    }

    [Test]
    public async Task UpdateTicket_WithAssociateWithDealId_AssociatesCorrectly()
    {
        options.AssociateWithDealId = dealId;

        var result = await HubSpot.UpdateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);

        var associations = JObject.Parse(await TestHelpers.GetTestTicketAssociations(ticketId, "deals", ApiKey, baseUrl, CancellationToken.None));
        var associatedIds = associations["results"]?.Select(r => r["toObjectId"]?.ToString());
        Assert.That(associatedIds, Does.Contain(dealId));
    }

    [Test]
    public async Task UpdateTicket_WithAssociateWithCompanyId_AssociatesCorrectly()
    {
        options.AssociateWithCompanyId = companyId;

        var result = await HubSpot.UpdateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);

        var associations = JObject.Parse(await TestHelpers.GetTestTicketAssociations(ticketId, "companies", ApiKey, baseUrl, CancellationToken.None));
        var associatedIds = associations["results"]?.Select(r => r["toObjectId"]?.ToString());
        Assert.That(associatedIds, Does.Contain(companyId));
    }

    [Test]
    public async Task UpdateTicket_MissingTicketId_ReturnsErrorWithoutThrowing()
    {
        options.ThrowErrorOnFailure = false;
        input.TicketId = string.Empty;

        var result = await HubSpot.UpdateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("TicketId is required"));
    }

    [Test]
    public void ParseErrorMessage_EmptyBody_ReturnsUnknownError()
    {
        var result = HubSpot.ParseErrorMessage(string.Empty);
        Assert.That(result, Is.EqualTo("Unknown error"));
    }
}
