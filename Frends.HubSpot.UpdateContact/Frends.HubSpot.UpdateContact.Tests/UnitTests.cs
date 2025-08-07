using System;
using System.Threading;
using System.Threading.Tasks;
using dotenv.net;
using Frends.HubSpot.UpdateContact.Definitions;
using Frends.HubSpot.UpdateContact.Helpers;
using Frends.HubSpot.UpdateContact.Tests.Helpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.HubSpot.UpdateContact.Tests;

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
            UpdateData = @"{
                ""email"": ""updated@example.com"",
                ""firstname"": ""Updated"",
                ""lastname"": ""User"",
            }",
        };

        options = new Options
        {
            ThrowErrorOnFailure = true,
            CheckIfExists = true,
        };
    }

    [TearDown]
    public async Task Cleanup()
    {
        if (contactId != null)
            await TestHelpers.DeleteTestContact(contactId, apiKey, baseUrl, false, CancellationToken.None);
    }

    [Test]
    public async Task UpdateContact_SuccessTest()
    {
        contactId = await TestHelpers.CreateTestContact(apiKey, baseUrl, CancellationToken.None);
        input.ContactId = contactId;

        var result = await HubSpot.UpdateContact(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);

        var contactData = JObject.Parse(await TestHelpers.GetTestContact(contactId, apiKey, baseUrl, CancellationToken.None));
        var properties = contactData["properties"];

        Assert.That(properties["email"]?.ToString(), Is.EqualTo("updated@example.com"));
        Assert.That(properties["firstname"]?.ToString(), Is.EqualTo("Updated"));
        Assert.That(properties["lastname"]?.ToString(), Is.EqualTo("User"));
    }

    [Test]
    public async Task UpdateContact_CheckIfExistsFalse_SuccessTest()
    {
        contactId = await TestHelpers.CreateTestContact(apiKey, baseUrl, CancellationToken.None);
        input.ContactId = contactId;

        options.CheckIfExists = false;
        var result = await HubSpot.UpdateContact(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
    }

    [Test]
    public void UpdateContact_ApiKeyValidationFailureTest()
    {
        var invalidConnection = new Connection
        {
            BaseUrl = baseUrl,
            ApiKey = null,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.UpdateContact(input, invalidConnection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("API Key is required"));
    }

    [Test]
    public void UpdateContact_BaseUrlValidationFailureTest()
    {
        var invalidConnection = new Connection
        {
            BaseUrl = null,
            ApiKey = apiKey,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.UpdateContact(input, invalidConnection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Base URL is required"));
    }

    [Test]
    public void UpdateContact_ContactIdValidationFailureTest()
    {
        var invalidInput = new Input
        {
            ContactId = null,
            UpdateData = input.UpdateData,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.UpdateContact(invalidInput, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("ContactId is required"));
    }

    [Test]
    public void UpdateContact_InvalidContactIdFormatTest()
    {
        var invalidInput = new Input
        {
            ContactId = "invalid_id",
            UpdateData = input.UpdateData,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.UpdateContact(invalidInput, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Contact ID should be a numeric value"));
    }

    [Test]
    public void UpdateContact_UpdateDataValidationFailureTest()
    {
        var invalidInput = new Input
        {
            ContactId = contactId,
            UpdateData = null,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.UpdateContact(invalidInput, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("UpdateData is required"));
    }

    [Test]
    public void UpdateContact_InvalidJsonValidationFailureTest()
    {
        var invalidInput = new Input
        {
            ContactId = contactId,
            UpdateData = "{ invalid json }",
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.UpdateContact(invalidInput, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Invalid JSON format in ContactData"));
    }

    [Test]
    public void UpdateContact_NonExistentContactTest()
    {
        input.ContactId = "999999999";
        options.CheckIfExists = true;

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.UpdateContact(input, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Contact with ID 999999999 does not exist"));
    }

    [Test]
    public async Task UpdateContact_NonExistentContactWithCheckFalse_ShouldNotThrow()
    {
        input.ContactId = "999999999";
        options.CheckIfExists = false;
        options.ThrowErrorOnFailure = false;

        var result = await HubSpot.UpdateContact(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public async Task UpdateContact_ErrorHandlingTest()
    {
        options.ThrowErrorOnFailure = false;
        options.ErrorMessageOnFailure = "Custom update error";

        var invalidInput = new Input
        {
            ContactId = contactId,
            UpdateData = @"{ ""invalid_property"": ""value"" }",
        };

        var result = await HubSpot.UpdateContact(invalidInput, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Does.Contain("Custom update error"));
    }

    [Test]
    public void ErrorHandler_Handle_WhenThrowErrorIsTrue_ThrowsException()
    {
        var ex = new Exception("Test exception");
        const string customMessage = "Custom error context";

        var thrownEx = Assert.Throws<Exception>(() => ErrorHandler.Handle(ex, true, customMessage));

        Assert.That(thrownEx.Message, Is.EqualTo($"{customMessage} {ex.Message}"));
        Assert.That(thrownEx.InnerException, Is.EqualTo(ex));
    }

    [Test]
    public void ErrorHandler_Handle_WhenThrowErrorIsFalse_ReturnsErrorResult()
    {
        var ex = new Exception("Test exception");
        const string customMessage = "Custom error context";

        var result = ErrorHandler.Handle(ex, false, customMessage);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Is.EqualTo($"{customMessage} {ex.Message}"));
        Assert.That(result.Error.AdditionalInfo, Is.EqualTo(ex));
    }
}