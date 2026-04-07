using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.HubSpot.DeleteCompany.Definitions;
using Frends.HubSpot.DeleteCompany.Tests.Helpers;
using NUnit.Framework;

namespace Frends.HubSpot.DeleteCompany.Tests;

[TestFixture]
public class FunctionalTests : TestBase
{
    private readonly string baseUrl = "https://api.hubapi.com";
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

        companyId = await TestHelpers.CreateTestCompany(ApiKey, baseUrl, CancellationToken.None);

        input = new Input
        {
            Id = companyId,
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
    public async Task DeleteCompany_SuccessTest()
    {
        var result = await HubSpot.DeleteCompany(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Id, Is.EqualTo(companyId));
        companyId = null;
    }

    [Test]
    public async Task DeleteCompany_NonExistentCompanyId_ReturnsSuccess()
    {
        input.Id = "000000000000";
        options.ThrowErrorOnFailure = false;

        var result = await HubSpot.DeleteCompany(input, connection, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
    }
}
