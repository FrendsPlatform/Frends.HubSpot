namespace Frends.HubSpot.CreateDeal.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// Result of creating a HubSpot deal.
    /// </summary>
    /// <param name="success">True if the operation succeeded.</param>
    /// <param name="id">Unique Id of the created deal.</param>
    /// <param name="error">Error details if the operation failed.</param>
    internal Result(bool success, string id, Error error = null)
    {
        Success = success;
        Id = id;
        Error = error;
    }

    /// <summary>
    /// Indicates whether deal creation was successful.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Unique Id of the created deal.
    /// </summary>
    /// <example>1234567890</example>
    public string Id { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, object { Exception Exception } AdditionalInfo }</example>
    public Error Error { get; set; }
}
