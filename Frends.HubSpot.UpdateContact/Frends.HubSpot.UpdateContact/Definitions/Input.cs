using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.UpdateContact.Definitions;

/// <summary>
/// Input parameters for updating a HubSpot contact.
/// </summary>
public class Input
{
    /// <summary>
    /// The unique Id of the contact.
    /// </summary>
    /// <example>1234567890</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string ContactId { get; set; }

    /// <summary>
    /// Properties to update as a Json string.
    /// </summary>
    /// <example>{ "lastname": "Doe" }</example>
    [DisplayFormat(DataFormatString = "Json")]
    public string UpdateData { get; set; }
}
