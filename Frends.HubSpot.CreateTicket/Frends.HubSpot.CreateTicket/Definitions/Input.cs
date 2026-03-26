using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HubSpot.CreateTicket.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Ticket properties such as subject, content, hs_pipeline, hs_pipeline_stage, hs_ticket_priority.
    /// </summary>
    /// <example>{ "subject": "Login issue", "content": "User cannot log in after password reset", "hs_pipeline": "0", "hs_pipeline_stage": "1", "hs_ticket_priority": "HIGH" }</example>
    [DisplayFormat(DataFormatString = "Json")]
    public string TicketData { get; set; }
}
