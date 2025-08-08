namespace Frends.HubSpot.UpdateContact.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// Result of updating a HubSpot contact.
    /// </summary>
    /// <param name="success">True if the operation succeeded.</param>
    /// <param name="error">Error details if the operation failed.</param>
    internal Result(bool success, Error error = null)
    {
        Success = success;
        Error = error;
    }

    /// <summary>
    /// Indicates whether contact creation was successful.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, object { Exception Exception } AdditionalInfo }</example>
    public Error Error { get; set; }
}
