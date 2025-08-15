using System;

namespace Frends.HubSpot.CreateDeal.Definitions;

/// <summary>
/// Error that occurred during the task.
/// </summary>
public class Error
{
    /// <summary>
    /// Summary of the error.
    /// </summary>
    /// <example>Unable to create the deal.</example>
    public string Message { get; set; }

    /// <summary>
    /// Additional information about the error.
    /// </summary>
    /// <example>object { Exception Exception }</example>
    public Exception AdditionalInfo { get; set; }
}
