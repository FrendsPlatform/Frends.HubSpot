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
    /// <param name="filterQuery">The filter query string</param>
    /// <returns>Tuple containing property name, operator and value</returns>
    public static (string PropertyName, string Operator, string Value) ParseFilterQuery(string filterQuery)
    {
        if (string.IsNullOrWhiteSpace(filterQuery))
            throw new ArgumentNullException(nameof(filterQuery), "Filter query cannot be null or whitespace.");

        var parts = filterQuery.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
            throw new Exception("Invalid filter format. Use: 'property operator \"value\"'");

        var operatorMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["eq"] = "EQ",
            ["ne"] = "NEQ",
            ["neq"] = "NEQ",
            ["gt"] = "GT",
            ["lt"] = "LT",
            ["gte"] = "GTE",
            ["lte"] = "LTE",
            ["between"] = "BETWEEN",
            ["in"] = "IN",
            ["not_in"] = "NOT_IN",
            ["has_property"] = "HAS_PROPERTY",
            ["not_has_property"] = "NOT_HAS_PROPERTY",
            ["contains"] = "CONTAINS_TOKEN",
            ["contains_token"] = "CONTAINS_TOKEN",
            ["not_contains_token"] = "NOT_CONTAINS_TOKEN",
        };

        if (string.IsNullOrWhiteSpace(parts[1]))
            throw new Exception("Operator cannot be null or whitespace.");

        if (!operatorMap.TryGetValue(parts[1].ToLower(), out var op))
            throw new Exception($"Unsupported operator: {parts[1]}");

        var value = string.Join(" ", parts.Skip(2)).Trim('\'', '"');

        return (parts[0], op, value);
    }
}