namespace Frends.HubSpot.CreateContact.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// Result of creating a HubSpot contact.
    /// </summary>
    /// <param name="success">True if the operation succeeded.</param>
    /// <param name="contactId">Unique Id of the created contact.</param>
    /// <param name="error">Error details if the operation failed.</param>
    internal Result(bool success, string contactId, Error error = null)
    {
        Success = success;
        ContactId = contactId;
        Error = error;
    }

    /// <summary>
    /// Indicates whether the retrieval was successful.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Unique Id of the created contact.
    /// </summary>
    /// <example>123456789</example>
    public string ContactId { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, object { Exception Exception } AdditionalInfo }</example>
    public Error Error { get; set; }
}
