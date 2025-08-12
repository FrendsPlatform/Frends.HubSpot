using System;
using System.Threading;
using System.Threading.Tasks;
using dotenv.net;
using Frends.HubSpot.CreateContact.Definitions;
using Frends.HubSpot.CreateContact.Helpers;
using Frends.HubSpot.CreateContact.Tests.Helpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.HubSpot.CreateContact.Tests;

/// <summary>
/// Test cases for HubSpot CreateContact task.
/// </summary>
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
            ContactData = @"{
                ""email"": ""test@example.com"",
                ""firstname"": ""Test"",
                ""lastname"": ""User"",
                ""phone"": ""+1234567890"",
            }",
        };

        options = new Options
        {
            ThrowErrorOnFailure = true,
            ValidateEmail = true,
        };
    }

    [TearDown]
    public async Task Cleanup()
    {
        if (contactId != null)
            await TestHelpers.DeleteTestContact(contactId, apiKey, baseUrl, true, CancellationToken.None);
    }

    [Test]
    public async Task CreateContact_SuccessTest()
    {
        var result = await HubSpot.CreateContact(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.ContactId, Is.Not.Null.And.Not.Empty);
        contactId = result.ContactId;

        var testContactData = JObject.Parse(await TestHelpers.GetTestContact(contactId, apiKey, baseUrl, CancellationToken.None));

        Assert.That(testContactData["id"]?.ToString(), Is.EqualTo(contactId));
        Assert.That(testContactData["properties"]?["email"]?.ToString(), Is.EqualTo("test@example.com"));
        Assert.That(testContactData["properties"]?["firstname"]?.ToString(), Is.EqualTo("Test"));
        Assert.That(testContactData["properties"]?["lastname"]?.ToString(), Is.EqualTo("User"));
    }

    [TestCase("hubspot.com")]
    [TestCase("example.com")]
    [TestCase("testcompany.org")]
    public async Task CreateContact_WithCompanyAssociation_SuccessTest(string domain)
    {
        string contactId = null;

        try
        {
            input.ContactData = $@"{{
            ""email"": ""testcontact@{domain}"",
            ""firstname"": ""Test"",
            ""lastname"": ""User""
        }}";

            var result = await HubSpot.CreateContact(input, connection, options, CancellationToken.None);

            Assert.That(result.Success, Is.True, $"Failed for {domain}");
            Assert.That(result.ContactId, Is.Not.Null.And.Not.Empty, $"No contact ID for {domain}");

            contactId = result.ContactId;
            var testContactData = JObject.Parse(await TestHelpers.GetTestContact(contactId, apiKey, baseUrl, CancellationToken.None));

            Assert.That(testContactData["properties"]?["email"]?.ToString(), Is.EqualTo($"testcontact@{domain}"));
            Assert.That(testContactData["properties"]?["firstname"]?.ToString(), Is.EqualTo("Test"));
            Assert.That(testContactData["properties"]?["lastname"]?.ToString(), Is.EqualTo("User"));
        }
        finally
        {
            if (contactId != null)
            {
                try
                {
                    await TestHelpers.DeleteTestContact(contactId, apiKey, baseUrl, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to clean up contact {contactId}: {ex.Message}");
                }
            }
        }
    }

    [Test]
    public void CreateContact_ApiKeyValidationFailureTest()
    {
        var invalidConnection = new Connection
        {
            BaseUrl = baseUrl,
            ApiKey = null,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.CreateContact(input, invalidConnection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("API Key is required"));
    }

    [Test]
    public void CreateContact_BaseUrlValidationFailureTest()
    {
        var invalidConnection = new Connection
        {
            BaseUrl = null,
            ApiKey = apiKey,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.CreateContact(input, invalidConnection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Base URL is required"));
    }

    [Test]
    public void CreateContact_ContactDataValidationFailureTest()
    {
        var invalidInput = new Input
        {
            ContactData = null,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.CreateContact(invalidInput, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("ContactData is required"));
    }

    [Test]
    public void CreateContact_InvalidJsonValidationFailureTest()
    {
        var invalidInput = new Input
        {
            ContactData = "{ invalid json }",
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.CreateContact(invalidInput, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Invalid JSON format in ContactData"));
    }

    [Test]
    public void CreateContact_InvalidEmailValidationFailureTest()
    {
        var invalidInput = new Input
        {
            ContactData = @"{ ""email"": ""invalid-email"" }",
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.CreateContact(invalidInput, connection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Invalid email format"));
    }

    [Test]
    public async Task CreateContact_ErrorHandlingTest()
    {
        options.ThrowErrorOnFailure = false;
        options.ErrorMessageOnFailure = "Custom error message";

        var invalidInput = new Input
        {
            ContactData = @"{ ""email"": ""duplicate@example.com"" }",
        };

        var firstResult = await HubSpot.CreateContact(invalidInput, connection, options, CancellationToken.None);
        contactId = firstResult.ContactId;

        var result = await HubSpot.CreateContact(invalidInput, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Does.Contain("Custom error message"));
    }

    [Test]
    public void CreateContact_Handle_WhenThrowErrorIsTrue_ThrowsException()
    {
        var ex = new Exception("Test exception");
        const string customMessage = "Custom error context";

        var thrownEx = Assert.Throws<Exception>(() => ErrorHandler.Handle(ex, true, customMessage));

        Assert.That(thrownEx.Message, Is.EqualTo($"{customMessage} {ex.Message}"));
        Assert.That(thrownEx.InnerException, Is.EqualTo(ex));
    }

    [Test]
    public void CreateContact_Handle_WhenThrowErrorIsFalse_ReturnsErrorResult()
    {
        var ex = new Exception("Test exception");
        const string customMessage = "Custom error context";

        var result = ErrorHandler.Handle(ex, false, customMessage);

        Assert.That(result.Success, Is.False);
        Assert.That(result.ContactId, Is.Null);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Is.EqualTo($"{customMessage} {ex.Message}"));
        Assert.That(result.Error.AdditionalInfo, Is.EqualTo(ex));
    }

    [Test]
    public void ValidationHelper_IsValidEmail_ValidEmailsTest()
    {
        Assert.That(ValidationHelper.IsValidEmail("test@example.com"), Is.True);
        Assert.That(ValidationHelper.IsValidEmail("first.last@sub.domain.com"), Is.True);
        Assert.That(ValidationHelper.IsValidEmail("user+tag@example.org"), Is.True);
    }

    [Test]
    public void ValidationHelper_IsValidEmail_InvalidEmailsTest()
    {
        Assert.That(ValidationHelper.IsValidEmail("plainstring"), Is.False);
        Assert.That(ValidationHelper.IsValidEmail("missing@domain"), Is.False);
        Assert.That(ValidationHelper.IsValidEmail("@missinglocal.com"), Is.False);
    }
}