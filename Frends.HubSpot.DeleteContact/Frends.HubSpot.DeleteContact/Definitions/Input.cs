using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.DeleteContact.Definitions;

/// <summary>
/// Input parameters for deleting a HubSpot contact.
/// </summary>
public class Input
{
    /// <summary>
    /// The unique Id of the contact to delete.
    /// </summary>
    /// <example>1234567890</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string ContactId { get; set; }
}
