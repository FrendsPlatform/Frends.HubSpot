using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.UpdateCompany.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// The HubSpot ID of the company to update.
    /// </summary>
    /// <example>foobar</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string Id { get; set; }

    /// <summary>
    /// Company properties to update such as name, domain, phone, industry.
    /// </summary>
    /// <example>{ "name": "Acme Corp", "domain": "acme.com", "phone": "123456789", "industry": "TECHNOLOGY" }</example>
    [DisplayFormat(DataFormatString = "Json")]
    public string CompanyData { get; set; }
}
