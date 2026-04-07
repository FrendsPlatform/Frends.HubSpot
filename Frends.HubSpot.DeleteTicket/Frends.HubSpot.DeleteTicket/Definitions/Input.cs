using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.DeleteTicket.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// The ID of the ticket to delete.
    /// </summary>
    /// <example>123456789</example>
    public string TicketId { get; set; }
}
