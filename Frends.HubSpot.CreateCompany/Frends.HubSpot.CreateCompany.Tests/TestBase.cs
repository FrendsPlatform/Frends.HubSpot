using System;
using dotenv.net;

namespace Frends.HubSpot.CreateCompany.Tests;

public abstract class TestBase
{
    protected TestBase()
    {
        DotEnv.Load();
        ApiKey = Environment.GetEnvironmentVariable("FRENDS_HubSpot_privateAccessToken")
            ?? throw new InvalidOperationException("Missing required env var: FRENDS_HubSpot_privateAccessToken");
    }

    protected string ApiKey { get; }
}
