using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.CreateCompany.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// HubSpot Private App access token.
    /// </summary>
    /// <example>xxx-xxx-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</example>
    [DisplayFormat(DataFormatString = "Text")]
    [PasswordPropertyText]
    public string ApiKey { get; set; }

    /// <summary>
    /// Base Url for the Api.
    /// </summary>
    /// <example>https://api.hubapi.com</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string BaseUrl { get; set; }
}
