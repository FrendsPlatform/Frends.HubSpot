using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.CreateTicket.Tests.Helpers;
using Frends.HubSpot.DeleteTicket.Definitions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.HubSpot.DeleteTicket.Tests;

[TestFixture]
public class FunctionalTests : TestBase
{
    private readonly string baseUrl = "https://api.hubapi.com";
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
                ""subject"": ""Ticket To Delete"",
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
        };
    }

    [TearDown]
    public async Task Cleanup()
    {
        if (ticketId != null)
        {
            try
            {
                await TestHelpers.DeleteTestTicket(ticketId, ApiKey, baseUrl, CancellationToken.None);
            }
            catch
            {
            }

            ticketId = null;
        }
    }

    [Test]
    public async Task DeleteTicket_SuccessTest()
    {
        var result = await HubSpot.DeleteTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Id, Is.EqualTo(ticketId));

        var exists = await TestHelpers.TicketExists(ticketId, ApiKey, baseUrl, CancellationToken.None);
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task DeleteTicket_NonExistentTicketId_ReturnsSuccess()
    {
        input.TicketId = "000000000000";

        var result = await HubSpot.DeleteTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
    }
}