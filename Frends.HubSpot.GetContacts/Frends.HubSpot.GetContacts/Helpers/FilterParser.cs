using System;
using System.Collections.Generic;
using System.Linq;

namespace Frends.HubSpot.GetContacts.Helpers;

/// <summary>
/// Helper methods for parsing filter queries
/// </summary>
public static class FilterParser
{
    /// <summary>
    /// Parses a filter query string into property name, operator and value
    /// </summary>
    /// <param name="filterQuery">The filter query string (e.g., "email eq 'test@example.com'")</param>
    /// <returns>Tuple containing property name, operator and value</returns>
    public static (string PropertyName, string Operator, string Value) ParseFilterQuery(string filterQuery)
    {
        var parts = filterQuery.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
            throw new Exception("Invalid filter format. Use: 'property eq \"value\"'");

        var operatorMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["eq"] = "EQ",
            ["ne"] = "NEQ",
            ["gt"] = "GT",
            ["lt"] = "LT",
            ["contains"] = "CONTAINS_TOKEN",
        };

        if (!operatorMap.TryGetValue(parts[1].ToLower(), out var op))
            throw new Exception($"Unsupported operator: {parts[1]}");

        var value = string.Join(" ", parts.Skip(2)).Trim('\'', '"');

        return (parts[0], op, value);
    }
}