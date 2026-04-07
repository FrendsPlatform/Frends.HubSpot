using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.DeleteCompany.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// The HubSpot ID of the company to delete.
    /// </summary>
    /// <example>123</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string Id { get; set; }
}
