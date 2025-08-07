using System.Net.Mail;

namespace Frends.HubSpot.CreateContact.Helpers;

/// <summary>
/// Validation helper methods
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates email address format
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if valid email format</returns>
    public static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email &&
                   email.Contains('.') &&
                   email.IndexOf('@') > 0 &&
                   email.IndexOf('@') < email.LastIndexOf(value: '.');
        }
        catch
        {
            return false;
        }
    }
}