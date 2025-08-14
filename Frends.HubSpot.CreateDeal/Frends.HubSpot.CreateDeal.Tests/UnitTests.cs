using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dotenv.net;
using Frends.HubSpot.CreateDeal.Definitions;
using Frends.HubSpot.CreateDeal.Helpers;
using Frends.HubSpot.CreateDeal.Tests.Helpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.HubSpot.CreateDeal.Tests;

/// <summary>
/// Test cases for HubSpot CreateDeal task.
/// </summary>
[TestFixture]
public class UnitTests
{
    private readonly string baseUrl = "https://api.hubapi.com";
    private readonly string apiKey;
    private string dealId;
    private Connection connection;
    private Input input;
    private Options options;

    public UnitTests()
    {
        DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
        apiKey = Environment.GetEnvironmentVariable("FRENDS_HubSpot_privateAccessToken");
    }

    [SetUp]
    public void Setup()
    {
        connection = new Connection
        {
            BaseUrl = baseUrl,
            ApiKey = apiKey,
        };

        input = new Input
        {
            DealData = @"{
            ""amount"": ""5000"", 
            ""dealname"": ""Test Deal"",
            ""dealstage"": ""presentationscheduled"",
            ""hs_deal_stage_probability"": ""0.5""
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
        if (dealId != null)
            await TestHelpers.DeleteTestDeal(dealId, apiKey, baseUrl, CancellationToken.None);
    }

    [Test]
    public async Task CreateDeal_SuccessTest()
    {
        var result = await HubSpot.CreateDeal(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Id, Is.Not.Null.And.Not.Empty);
        dealId = result.Id;

        var testDealData = JObject.Parse(await TestHelpers.GetTestDeal(dealId, apiKey, baseUrl, CancellationToken.None));

        Assert.That(testDealData["id"]?.ToString(), Is.EqualTo(dealId));
        Assert.That(testDealData["properties"]?["amount"]?.ToString(), Is.EqualTo("5000"));
        Assert.That(testDealData["properties"]?["dealname"]?.ToString(), Is.EqualTo("Test Deal"));
    }

    [Test]
    public async Task CreateDeal_WithContactAssociation_SuccessTest()
    {
        var contactId = await TestHelpers.CreateTestContact(apiKey, baseUrl, CancellationToken.None);
        Assert.That(contactId, Is.Not.Null.And.Not.Empty);

        options.AssociateWithContactId = contactId;

        var result = await HubSpot.CreateDeal(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Id, Is.Not.Null.And.Not.Empty);
        dealId = result.Id;

        var dealResponse = await TestHelpers.GetTestDeal(dealId, apiKey, baseUrl, CancellationToken.None);
        var dealData = JObject.Parse(dealResponse);

        var associatedContacts = dealData["associations"]?["contacts"]?["results"];
        bool foundAssociation = false;

        if (associatedContacts != null)
        {
            foundAssociation = associatedContacts
                .Any(c => c["id"]?.ToString() == contactId);
        }

        Assert.That(foundAssociation, Is.True, $"Deal {dealId} should be associated with contact {contactId}");

        await TestHelpers.DeleteTestContact(contactId, apiKey, baseUrl, true, CancellationToken.None);
    }

    [Test]
    public void CreateDeal_ApiKeyValidationFailureTest()
    {
        var invalidConnection = new Connection
        {
            BaseUrl = baseUrl,
            ApiKey = null,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.CreateDeal(input, invalidConnection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("API Key is required"));
    }

    [Test]
    public void CreateDeal_BaseUrlValidationFailureTest()
    {
        var invalidConnection = new Connection
        {
            BaseUrl = null,
            ApiKey = apiKey,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.CreateDeal(input, invalidConnection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Base URL is required"));
    }

    [Test]
    public void CreateDeal_DealDataValidationFailureTest()
    {
        var invalidInput = new Input
        {
            DealData = null,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.CreateDeal(invalidInput, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("DealData is required"));
    }

    [Test]
    public void CreateDeal_InvalidJsonValidationFailureTest()
    {
        var invalidInput = new Input
        {
            DealData = "{ invalid json }",
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.CreateDeal(invalidInput, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Invalid JSON format in DealData"));
    }

    [Test]
    public void CreateDeal_Handle_WhenThrowErrorIsTrue_ThrowsException()
    {
        var ex = new Exception("Test exception");
        const string customMessage = "Custom error context";

        var thrownEx = Assert.Throws<Exception>(() => ErrorHandler.Handle(ex, true, customMessage));

        Assert.That(thrownEx.Message, Is.EqualTo($"{customMessage} {ex.Message}"));
        Assert.That(thrownEx.InnerException, Is.EqualTo(ex));
    }

    [Test]
    public void CreateDeal_Handle_WhenThrowErrorIsFalse_ReturnsErrorResult()
    {
        var ex = new Exception("Test exception");
        const string customMessage = "Custom error context";

        var result = ErrorHandler.Handle(ex, false, customMessage);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Id, Is.Null);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Is.EqualTo($"{customMessage} {ex.Message}"));
        Assert.That(result.Error.AdditionalInfo, Is.EqualTo(ex));
    }
}