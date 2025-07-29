using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using dotenv.net;
using Frends.HubSpot.DeleteContact.Definitions;
using Frends.HubSpot.DeleteContact.Tests.Helpers;
using NUnit.Framework;

namespace Frends.HubSpot.DeleteContact.Tests;

[TestFixture]
public class UnitTests
{
    private readonly string baseUrl = "https://api.hubapi.com";
    private readonly string apiKey;
    private string contactId;
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
            ContactId = contactId,
        };

        options = new Options
        {
            ThrowErrorOnFailure = true,
        };
    }

    [Test]
    public async Task DeleteContact_SuccessTest()
    {
        contactId = await TestHelpers.CreateTestContact(apiKey, baseUrl, CancellationToken.None);
        input.ContactId = contactId;

        var result = await HubSpot.DeleteContact(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        var response = await client.GetAsync($"{baseUrl}/crm/v3/objects/contacts/{contactId}", CancellationToken.None);
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }

    [Test]
    public void DeleteContact_ApiKeyValidationFailureTest()
    {
        var invalidConnection = new Connection
        {
            BaseUrl = baseUrl,
            ApiKey = null,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.DeleteContact(input, invalidConnection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("API Key is required"));
    }

    [Test]
    public void DeleteContact_BaseUrlValidationFailureTest()
    {
        var invalidConnection = new Connection
        {
            BaseUrl = null,
            ApiKey = apiKey,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.DeleteContact(input, invalidConnection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Base URL is required"));
    }

    [Test]
    public void DeleteContact_ContactIdValidationFailureTest()
    {
        var invalidInput = new Input
        {
            ContactId = null,
        };

        var ex = Assert.ThrowsAsync<Exception>(() =>
            HubSpot.DeleteContact(invalidInput, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("ContactId is required"));
    }

    [Test]
    public async Task DeleteContact_NonExistentContactTest()
    {
        input.ContactId = "999999999";
        var result = await HubSpot.DeleteContact(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task DeleteContact_InvalidIdFormatTest()
    {
        input.ContactId = "invalid_id_format";
        options.ThrowErrorOnFailure = false;

        var result = await HubSpot.DeleteContact(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Does.Contain("Contact ID should be a numeric value"));
    }

    [Test]
    public void DeleteContact_InvalidIdFormatThrowsTest()
    {
        input.ContactId = "invalid_id_format";
        options.ThrowErrorOnFailure = true;

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.DeleteContact(input, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Contact ID should be a numeric value:"));
    }

    [Test]
    public async Task DeleteContact_ErrorHandlingTest()
    {
        options.ThrowErrorOnFailure = false;
        options.ErrorMessageOnFailure = "Custom delete error";

        input.ContactId = "invalid_id_format";

        var result = await HubSpot.DeleteContact(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Does.Contain("Custom delete error"));
    }
}