using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.CreateDeal.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Optional. Contact Id to associate the deal with.
    /// </summary>
    /// <example>1234567890</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string AssociateWithContactId { get; set; }

    /// <summary>
    /// Whether to throw an error on failure. True by default.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Failed to create the deal.</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string ErrorMessageOnFailure { get; set; }
}