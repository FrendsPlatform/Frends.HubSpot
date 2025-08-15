using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using dotenv.net;
using Frends.HubSpot.GetContacts.Definitions;
using Frends.HubSpot.GetContacts.Helpers;
using Frends.HubSpot.GetContacts.Tests.Helpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.HubSpot.GetContacts.Tests;

/// <summary>
/// Test cases for HubSpot GetContacts task.
/// </summary>
[TestFixture]
public class UnitTests
{
    private readonly string baseUrl = "https://api.hubapi.com";
    private readonly string apiKey;
    private Connection connection;
    private Input input;
    private Options options;
    private List<(string Id, string Email)> testContacts = [];

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
            Properties = ["email", "firstname", "lastname"],
            Limit = 5,
        };

        options = new Options
        {
            ThrowErrorOnFailure = true,
        };
    }

    [TearDown]
    public async Task Cleanup()
    {
        foreach (var (id, _) in testContacts)
        {
            try
            {
                await TestHelpers.DeleteTestContact(id, apiKey, baseUrl, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete test contact {id}: {ex.Message}");
            }
        }

        testContacts.Clear();
    }

    [Test]
    public async Task GetContacts_SuccessTest()
    {
        await CreateTestContacts(3);
        var result = await HubSpot.GetContacts(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Contacts, Is.Not.Null);
        Assert.That(result.Contacts.Count, Is.GreaterThanOrEqualTo(testContacts.Count));
    }

    [Test]
    public async Task GetContacts_WithSpecificProperties_SuccessTest()
    {
        input.Properties = ["email", "firstname"];

        var result = await HubSpot.GetContacts(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Contacts, Is.Not.Null);

        if (result.Contacts is JArray contactsArray && contactsArray.Count > 0)
        {
            var firstContact = contactsArray[0];
            Assert.That(firstContact["properties"]?["email"], Is.Not.Null);
            Assert.That(firstContact["properties"]?["firstname"], Is.Not.Null);
            Assert.That(firstContact["properties"]?["lastname"], Is.Null);
        }
    }

    [Test]
    public async Task GetContacts_WithFilterQuery_SuccessTest()
    {
        await CreateTestContacts(1);
        var (expectedId, expectedEmail) = testContacts[0];

        var maxRetries = 5;
        Result result = null;

        for (var i = 0; i < maxRetries; i++)
        {
            input.FilterQuery = $"email eq '{expectedEmail}'";
            input.Limit = 1;

            result = await HubSpot.GetContacts(input, connection, options, CancellationToken.None);

            if (result.Contacts.Count() == 1)
                break;

            await Task.Delay(2000 * i);
        }

        Assert.That(result.Success, Is.True);
        Assert.That(result.Contacts, Is.Not.Null);
        Assert.That(result.Contacts.Count(), Is.EqualTo(1), $"Failed to find contact after {maxRetries} attempts");

        var resultId = result.Contacts.First()["id"]?.ToString();
        var resultEmail = result.Contacts.First()["properties"]?["email"]?.ToString();

        Assert.That(resultId, Is.EqualTo(expectedId));
        Assert.That(resultEmail, Is.EqualTo(expectedEmail));
    }

    [Test]
    public async Task GetContacts_WithNotEqualFilter_SuccessTest()
    {
        await CreateTestContacts(2);
        var (excludedId, excludedEmail) = testContacts[0];
        var (expectedId, expectedEmail) = testContacts[1];

        var maxRetries = 5;
        Result result = null;

        for (var i = 0; i < maxRetries; i++)
        {
            input.FilterQuery = $"email neq '{excludedEmail}'";
            result = await HubSpot.GetContacts(input, connection, options, CancellationToken.None);

            if (result.Contacts.Any(c => c["id"]?.ToString() == expectedId))
                break;

            await Task.Delay(2000 * i);
        }

        Assert.That(result.Success, Is.True);
        Assert.That(result.Contacts.Any(c => c["id"]?.ToString() == expectedId), Is.True);
        Assert.That(result.Contacts.Any(c => c["id"]?.ToString() == excludedId), Is.False);
    }

    [Test]
    public async Task GetContacts_WithContainsFilter_SuccessTest()
    {
        await CreateTestContacts(1);
        var (expectedId, expectedEmail) = testContacts[0];
        var emailPrefix = expectedEmail.Split('@')[0];

        var maxRetries = 5;
        Result result = null;

        for (var i = 0; i < maxRetries; i++)
        {
            input.FilterQuery = $"email contains_token '{emailPrefix}'";
            result = await HubSpot.GetContacts(input, connection, options, CancellationToken.None);

            if (result.Contacts.Any())
                break;

            await Task.Delay(2000 * i);
        }

        Assert.That(result.Success, Is.True);
        Assert.That(result.Contacts.First()["id"]?.ToString(), Is.EqualTo(expectedId));
    }

    [Test]
    public async Task GetContacts_WithHasPropertyFilter_SuccessTest()
    {
        await CreateTestContacts(1);
        var (expectedId, expectedEmail) = testContacts[0];

        var maxRetries = 5;
        Result result = null;

        for (var i = 0; i < maxRetries; i++)
        {
            input.FilterQuery = "email has_property";
            result = await HubSpot.GetContacts(input, connection, options, CancellationToken.None);

            if (result.Contacts.Any(c => c["id"]?.ToString() == expectedId))
                break;

            await Task.Delay(2000 * i);
        }

        Assert.That(result.Success, Is.True);
        Assert.That(result.Contacts.Any(c => c["id"]?.ToString() == expectedId), Is.True);
    }

    [Test]
    public async Task GetContacts_WithLimit_SuccessTest()
    {
        await CreateTestContacts(2);
        input.Limit = 2;

        var result = await HubSpot.GetContacts(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Contacts, Is.Not.Null);
        Assert.That(result.Contacts.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetContacts_WithPagination_SuccessTest()
    {
        await CreateTestContacts(3);
        input.Limit = 2;

        var firstPage = await HubSpot.GetContacts(input, connection, options, CancellationToken.None);
        Assert.That(firstPage.Success, Is.True);
        Assert.That(firstPage.Contacts, Is.Not.Null);
        Assert.That(firstPage.Contacts.Count, Is.EqualTo(2));

        if (firstPage.HasMore)
        {
            input.After = firstPage.NextPageCursor;
            var secondPage = await HubSpot.GetContacts(input, connection, options, CancellationToken.None);

            Assert.That(secondPage.Success, Is.True);
            Assert.That(secondPage.Contacts, Is.Not.Null);
            Assert.That(secondPage.Contacts.Count, Is.GreaterThanOrEqualTo(1));
        }
    }

    [Test]
    public void GetContacts_ApiKeyValidationFailureTest()
    {
        var invalidConnection = new Connection
        {
            BaseUrl = baseUrl,
            ApiKey = null,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.GetContacts(input, invalidConnection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("API Key is required"));
    }

    [Test]
    public void GetContacts_BaseUrlValidationFailureTest()
    {
        var invalidConnection = new Connection
        {
            BaseUrl = null,
            ApiKey = apiKey,
        };

        var ex = Assert.ThrowsAsync<Exception>(() => HubSpot.GetContacts(input, invalidConnection, options, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Base URL is required"));
    }

    [Test]
    public async Task GetContacts_ErrorHandlingTest()
    {
        options.ThrowErrorOnFailure = false;
        options.ErrorMessageOnFailure = "Custom error message";

        input.FilterQuery = "invalid_property eq 'value'";

        var result = await HubSpot.GetContacts(input, connection, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Does.Contain("Custom error message"));
        Assert.That(result.Error.Message, Does.Contain("HubSpot API error"));
    }

    [Test]
    public void Handle_WhenThrowErrorIsTrue_ThrowsException()
    {
        var ex = new Exception("Test exception");
        const string customMessage = "Custom error context";

        var thrownEx = Assert.Throws<Exception>(() => ErrorHandler.Handle(ex, true, customMessage));

        Assert.That(thrownEx.Message, Is.EqualTo($"{customMessage} {ex.Message}"));
        Assert.That(thrownEx.InnerException, Is.EqualTo(ex));
    }

    [Test]
    public void Handle_WhenThrowErrorIsFalse_ReturnsErrorResult()
    {
        var ex = new Exception("Test exception");
        const string customMessage = "Custom error context";

        var result = ErrorHandler.Handle(ex, false, customMessage);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Contacts, Is.Null);
        Assert.That(result.HasMore, Is.False);
        Assert.That(result.NextPageCursor, Is.Null);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Is.EqualTo($"{customMessage} {ex.Message}"));
        Assert.That(result.Error.AdditionalInfo, Is.EqualTo(ex));
    }

    [Test]
    public async Task GetContacts_IncludeArchived_SuccessTest()
    {
        await CreateTestContacts(1);
        var (expectedId, expectedEmail) = testContacts[0];

        await TestHelpers.ArchiveTestContact(expectedId, apiKey, baseUrl, CancellationToken.None);

        input.FilterQuery = null;
        input.Limit = 20;
        options.IncludeArchived = true;

        var maxRetries = 5;
        JToken foundContact = null;
        Result archivedResult = null;

        for (int i = 0; i < maxRetries; i++)
        {
            archivedResult = await HubSpot.GetContacts(input, connection, options, CancellationToken.None);
            Assert.That(archivedResult.Success, Is.True);

            foundContact = archivedResult.Contacts.FirstOrDefault(c => c["properties"]?["email"]?.ToString() == expectedEmail);

            if (foundContact != null)
                break;

            await Task.Delay(2000 * i);
        }

        Assert.That(foundContact, Is.Not.Null, $"Archived contact {expectedEmail} not found after {maxRetries} attempts");
        Assert.That(foundContact["id"]?.ToString(), Is.EqualTo(expectedId));
        Assert.That(foundContact["properties"]?["email"]?.ToString(), Is.EqualTo(expectedEmail));
    }

    [Test]
    public void ParseFilterQuery_ShouldParseSingleValueOperator()
    {
        const string filter = "age gt 25";

        var result = FilterParser.ParseFilterQuery(filter);

        Assert.That(result.PropertyName, Is.EqualTo("age"));
        Assert.That(result.Operator, Is.EqualTo("GT"));
        Assert.That(result.Value, Is.EqualTo("25"));
        Assert.That(result.Values, Is.Null);
        Assert.That(result.HighValue, Is.Null);
    }

    [Test]
    public void ParseFilterQuery_ShouldParseInOperator()
    {
        const string filter = "status in active,pending";

        var result = FilterParser.ParseFilterQuery(filter);

        Assert.That(result.PropertyName, Is.EqualTo("status"));
        Assert.That(result.Operator, Is.EqualTo("IN"));
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Values, Is.EquivalentTo(["active", "pending"]));
        Assert.That(result.HighValue, Is.Null);
    }

    [Test]
    public void ParseFilterQuery_ShouldParseBetweenOperator()
    {
        const string filter = "price between 100,500";

        var result = FilterParser.ParseFilterQuery(filter);

        Assert.That(result.PropertyName, Is.EqualTo("price"));
        Assert.That(result.Operator, Is.EqualTo("BETWEEN"));
        Assert.That(result.Value, Is.EqualTo("100"));
        Assert.That(result.HighValue, Is.EqualTo("500"));
        Assert.That(result.Values, Is.Null);
    }

    [Test]
    public void ParseFilterQuery_ShouldParseHasPropertyOperator()
    {
        const string filter = "email has_property";

        var result = FilterParser.ParseFilterQuery(filter);

        Assert.That(result.PropertyName, Is.EqualTo("email"));
        Assert.That(result.Operator, Is.EqualTo("HAS_PROPERTY"));
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Values, Is.Null);
        Assert.That(result.HighValue, Is.Null);
    }

    [Test]
    public void ParseFilterQuery_ShouldThrowOnInvalidBetweenFormat()
    {
        const string filter = "price between 100";

        var ex = Assert.Throws<ArgumentException>(() => FilterParser.ParseFilterQuery(filter));
        Assert.That(ex.Message, Does.Contain("BETWEEN operator requires two non-empty comma-separated values"));
    }

    private async Task CreateTestContacts(int count)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        for (int i = 0; i < count; i++)
        {
            string contactId = null;

            try
            {
                contactId = await TestHelpers.CreateTestContact(apiKey, baseUrl, CancellationToken.None);
                var contactEmail = await TestHelpers.GetContactEmail(client, baseUrl, contactId, CancellationToken.None);

                if (string.IsNullOrEmpty(contactEmail))
                    throw new Exception($"Failed to retrieve email for test contact ID: {contactId}");

                testContacts.Add((contactId, contactEmail));
            }
            catch
            {
                if (contactId != null)
                {
                    try
                    {
                        await TestHelpers.DeleteTestContact(contactId, apiKey, baseUrl, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to cleanup failed test contact {contactId}: {ex.Message}");
                    }
                }

                throw;
            }
        }
    }
}