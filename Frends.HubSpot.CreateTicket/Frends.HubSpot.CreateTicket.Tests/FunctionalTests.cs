using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.CreateTicket.Definitions;
using Frends.HubSpot.CreateTicket.Tests.Helpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.HubSpot.CreateTicket.Tests;

[TestFixture]
public class FunctionalTests : TestBase
{
    private readonly string baseUrl = "https://api.hubapi.com";
    private string ticketId;
    private Connection connection;
    private Input input;
    private Options options;

    [SetUp]
    public void Setup()
    {
        connection = new Connection
        {
            BaseUrl = baseUrl,
            ApiKey = ApiKey,
        };
        input = new Input
        {
            TicketData = @"{
            ""subject"": ""Test Ticket"",
            ""content"": ""This is a test ticket created by automated tests"",
            ""hs_pipeline"": ""0"",
            ""hs_pipeline_stage"": ""1"",
            ""hs_ticket_priority"": ""MEDIUM""
        }",
        };
        options = new Options
        {
            ThrowErrorOnFailure = true,
        };
    }

    [TearDown]
    public async Task Cleanup()
    {
        if (ticketId != null)
            await TestHelpers.DeleteTestTicket(ticketId, ApiKey, baseUrl, CancellationToken.None);
    }

    [Test]
    public async Task CreateTicket_SuccessTest()
    {
        var result = await HubSpot.CreateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Id, Is.Not.Null.And.Not.Empty);
        ticketId = result.Id;

        var ticketData = JObject.Parse(await TestHelpers.GetTestTicket(ticketId, ApiKey, baseUrl, CancellationToken.None));
        Assert.That(ticketData["id"]?.ToString(), Is.EqualTo(ticketId));
        Assert.That(ticketData["properties"]?["subject"]?.ToString(), Is.EqualTo("Test Ticket"));
        Assert.That(ticketData["properties"]?["content"]?.ToString(), Is.EqualTo("This is a test ticket created by automated tests"));
    }

    [Test]
    public async Task CreateTicket_HighPriority_SetsCorrectly()
    {
        input.TicketData = @"{
        ""subject"": ""High Priority Ticket"",
        ""hs_pipeline"": ""0"",
        ""hs_pipeline_stage"": ""1"",
        ""hs_ticket_priority"": ""HIGH""
    }";

        var result = await HubSpot.CreateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        ticketId = result.Id;

        var ticketData = JObject.Parse(await TestHelpers.GetTestTicket(ticketId, ApiKey, baseUrl, CancellationToken.None));
        Assert.That(ticketData["properties"]?["hs_ticket_priority"]?.ToString(), Is.EqualTo("HIGH"));
    }

    [Test]
    public async Task CreateTicket_LowPriority_SetsCorrectly()
    {
        input.TicketData = @"{
        ""subject"": ""Low Priority Ticket"",
        ""hs_pipeline"": ""0"",
        ""hs_pipeline_stage"": ""1"",
        ""hs_ticket_priority"": ""LOW""
    }";

        var result = await HubSpot.CreateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        ticketId = result.Id;

        var ticketData = JObject.Parse(await TestHelpers.GetTestTicket(ticketId, ApiKey, baseUrl, CancellationToken.None));
        Assert.That(ticketData["properties"]?["hs_ticket_priority"]?.ToString(), Is.EqualTo("LOW"));
    }

    [Test]
    public async Task CreateTicket_WithCategory_SetsCorrectly()
    {
        input.TicketData = @"{
        ""subject"": ""Product Issue Ticket"",
        ""hs_pipeline"": ""0"",
        ""hs_pipeline_stage"": ""1"",
        ""hs_ticket_category"": ""PRODUCT_ISSUE""
    }";

        var result = await HubSpot.CreateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        ticketId = result.Id;

        var ticketData = JObject.Parse(await TestHelpers.GetTestTicket(ticketId, ApiKey, baseUrl, CancellationToken.None));
        Assert.That(ticketData["properties"]?["hs_ticket_category"]?.ToString(), Is.EqualTo("PRODUCT_ISSUE"));
    }

    [Test]
    public async Task CreateTicket_WithAssociateWithContactId_AssociatesCorrectly()
    {
        options.AssociateWithContactId = "335401013465";

        var result = await HubSpot.CreateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        ticketId = result.Id;

        var associations = JObject.Parse(await TestHelpers.GetTestTicketAssociations(ticketId, "contacts", ApiKey, baseUrl, CancellationToken.None));
        var associatedIds = associations["results"]?.Select(r => r["toObjectId"]?.ToString());
        Assert.That(associatedIds, Does.Contain("335401013465"));
    }

    [Test]
    public async Task CreateTicket_WithAssociateWithDealId_AssociatesCorrectly()
    {
        options.AssociateWithDealId = "494473663705";

        var result = await HubSpot.CreateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        ticketId = result.Id;

        var associations = JObject.Parse(await TestHelpers.GetTestTicketAssociations(ticketId, "deals", ApiKey, baseUrl, CancellationToken.None));
        var associatedIds = associations["results"]?.Select(r => r["toObjectId"]?.ToString());
        Assert.That(associatedIds, Does.Contain("494473663705"));
    }

    [Test]
    public async Task CreateTicket_WithAssociateWithCompanyId_AssociatesCorrectly()
    {
        options.AssociateWithCompanyId = "175778808024";

        var result = await HubSpot.CreateTicket(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        ticketId = result.Id;

        var associations = JObject.Parse(await TestHelpers.GetTestTicketAssociations(ticketId, "companies", ApiKey, baseUrl, CancellationToken.None));
        var associatedIds = associations["results"]?.Select(r => r["toObjectId"]?.ToString());
        Assert.That(associatedIds, Does.Contain("175778808024"));
    }
}
