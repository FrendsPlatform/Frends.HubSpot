using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.UpdateTicket.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// The ID of the ticket to update.
    /// </summary>
    /// <example>123456789</example>
    public string TicketId { get; set; }

    /// <summary>
    /// Ticket properties to update as a JSON object.
    /// </summary>
    /// <example>{ "subject": "Updated subject", "hs_ticket_priority": "HIGH" }</example>
    [DisplayFormat(DataFormatString = "Json")]
    public string TicketData { get; set; }
}
