using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.CreateContact.Definitions;

/// <summary>
/// Input parameters for creating a HubSpot contact.
/// </summary>
public class Input
{
    /// <summary>
    /// Key-value pairs representing contact properties as a JSON string.
    /// </summary>
    /// <example>{ "email": "john@example.com", "firstname": "John" }</example>
    [DisplayFormat(DataFormatString = "Json")]
    public string ContactData { get; set; }
}