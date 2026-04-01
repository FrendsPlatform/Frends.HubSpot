using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.CreateCompany.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Company properties such as name, domain, phone, industry.
    /// </summary>
    /// <example>{ "name": "Acme Corp", "domain": "acme.com", "phone": "123456789", "industry": "TECHNOLOGY" }</example>
    [DisplayFormat(DataFormatString = "Json")]
    public string CompanyData { get; set; }
}
