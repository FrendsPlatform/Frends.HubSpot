using Newtonsoft.Json.Linq;

namespace Frends.HubSpot.GetContacts.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// Result of retrieving HubSpot contacts.
    /// </summary>
    /// <param name="success">True if the operation succeeded.</param>
    /// <param name="contacts">Array of contact objects.</param>
    /// <param name="hasMore">Whether there are more results available.</param>
    /// <param name="nextPageCursor">Cursor for next page of results.</param>
    /// <param name="error">Error details if the operation failed.</param>
    internal Result(bool success, JToken contacts = null, bool hasMore = false, string nextPageCursor = null, Error error = null)
    {
        Success = success;
        Contacts = contacts;
        HasMore = hasMore;
        NextPageCursor = nextPageCursor;
        Error = error;
    }

    /// <summary>
    /// Indicates if the operation completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Array of contact objects.
    /// </summary>
    /// <example>[{ "id": "1", "properties": { "email": "test@example.com", "firstname": "John" } }]</example>
    public JToken Contacts { get; set; }

    /// <summary>
    /// Whether there are more results to retrieve.
    /// </summary>
    /// <example>true</example>
    public bool HasMore { get; set; }

    /// <summary>
    /// Cursor for next page of results, if any.
    /// </summary>
    /// <example>"MjAyMy0wMS0wMVQwMDowMDowMC4wMDAtMTI6MDA"</example>
    public string NextPageCursor { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, object { Exception Exception } AdditionalInfo }</example>
    public Error Error { get; set; }
}
