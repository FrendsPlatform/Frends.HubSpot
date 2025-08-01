using System.ComponentModel;

namespace Frends.HubSpot.GetContacts.Definitions;

/// <summary>
/// Input parameters for retrieving HubSpot contacts.
/// </summary>
public class Input
{
    /// <summary>
    /// Optional. Filter query to narrow search results in a simple expression format: `property operator 'value'`
    /// </summary>
    /// <example>email eq 'test@example.com'</example>
    public string FilterQuery { get; set; }

    /// <summary>
    /// Optional. Specific contact properties to retrieve.
    /// </summary>
    /// <example>new string[] { "email", "firstname", "lastname", "company" }</example>
    public string[] Properties { get; set; }

    /// <summary>
    /// Optional. Maximum number of contacts to return.
    /// </summary>
    /// <example>50</example>
    [DefaultValue(100)]
    public int Limit { get; set; } = 100;

    /// <summary>
    /// Optional. Paging cursor for fetching the next page.
    /// </summary>
    /// <example>"MjAyMy0wMS0wMVQwMDowMDowMC4wMDAtMTI6MDA"</example>
    public string After { get; set; }
}
