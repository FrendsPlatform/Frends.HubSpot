using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.CreateDeal.Definitions;

/// <summary>
/// Input parameters for creating a HubSpot deal.
/// </summary>
public class Input
{
    /// <summary>
    /// Deal properties such as amount, dealname, dealstage, pipeline.
    /// </summary>
    /// <example>{ "amount": "5000", "dealname": "Enterprise Deal", "dealstage": "presentation scheduled", "pipeline": "default" }</example>
    [DisplayFormat(DataFormatString = "Json")]
    public string DealData { get; set; }
}