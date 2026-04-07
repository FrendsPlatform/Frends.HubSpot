using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.UpdateCompany.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Associate the company with a HubSpot contact by providing the contact ID.
    /// </summary>
    /// <example>12345678</example>
    public string AssociateWithContactId { get; set; }

    /// <summary>
    /// Associate the company with a HubSpot deal by providing the deal ID.
    /// </summary>
    /// <example>98765432</example>
    public string AssociateWithDealId { get; set; }

    /// <summary>
    /// Associate the company with a HubSpot ticket by providing the ticket ID.
    /// </summary>
    /// <example>11223344</example>
    public string AssociateWithTicketId { get; set; }

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Custom error message</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
