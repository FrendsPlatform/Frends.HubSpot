using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.CreateContact.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Optional. Validate email format before sending. Default is false.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool ValidateEmail { get; set; } = false;

    /// <summary>
    /// Whether to throw an error on failure. True by default.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Failed to create contact.</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string ErrorMessageOnFailure { get; set; }
}
