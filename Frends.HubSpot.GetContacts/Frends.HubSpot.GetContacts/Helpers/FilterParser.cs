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
    /// Parses a filter query string into a ParsedFilter object compatible with HubSpot's search API.
    /// </summary>
    /// <param name="filterQuery">The filter string in different formats.</param>
    /// <returns>A ParsedFilter object containing the parsed property name, operator, and value(s).</returns>
    public static ParsedFilter ParseFilterQuery(string filterQuery)
    {
        if (string.IsNullOrWhiteSpace(filterQuery))
            throw new ArgumentNullException(nameof(filterQuery), "Filter query cannot be null or whitespace.");

        var parts = filterQuery.Split([' '], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
            throw new ArgumentException("Invalid filter format. Use: 'property operator \"value\"' for value-based operators, or 'property has_property'.", nameof(filterQuery));

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

        var propertyName = parts[0];
        var operatorKey = parts[1].ToLower();

        if (!operatorMap.TryGetValue(operatorKey, out var op))
            throw new ArgumentException($"Unsupported operator: {parts[1]}", nameof(filterQuery));

        var result = new ParsedFilter
        {
            PropertyName = propertyName,
            Operator = op,
        };

        var valuePart = string.Join(" ", parts.Skip(2)).Trim('\'', '"');

        switch (op)
        {
            case "IN":
            case "NOT_IN":
                if (string.IsNullOrWhiteSpace(valuePart))
                    throw new ArgumentException("IN/NOT_IN operators require at least one comma-separated value.", nameof(filterQuery));
                result.Values = [.. valuePart.Split(',', StringSplitOptions.TrimEntries).Where(v => !string.IsNullOrWhiteSpace(v))];
                if (result.Values.Count == 0)
                    throw new ArgumentException("IN/NOT_IN operators require at least one non-empty value.", nameof(filterQuery));
                break;
            case "BETWEEN":
                var bounds = valuePart.Split(',', StringSplitOptions.TrimEntries);
                if (bounds.Length != 2 || string.IsNullOrWhiteSpace(bounds[0]) || string.IsNullOrWhiteSpace(bounds[1]))
                    throw new ArgumentException("BETWEEN operator requires two non-empty comma-separated values.", nameof(filterQuery));
                result.Value = bounds[0];
                result.HighValue = bounds[1];
                break;
            case "HAS_PROPERTY":
            case "NOT_HAS_PROPERTY":
                if (!string.IsNullOrWhiteSpace(valuePart))
                    throw new ArgumentException($"{op} does not accept a value.", nameof(filterQuery));
                break;
            default:
                if (string.IsNullOrWhiteSpace(valuePart))
                    throw new ArgumentException($"{op} operator requires a value.", nameof(filterQuery));
                result.Value = valuePart;
                break;
        }

        return result;
    }

    /// <summary>
    /// Represents the parsed components of a HubSpot filter query.
    /// </summary>
    public class ParsedFilter
    {
        /// <summary>
        /// The name of the HubSpot property being filtered.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// HubSpot operator (e.g., EQ, IN, BETWEEN).
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// The main value for the filter.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The second value used in BETWEEN filters.
        /// </summary>
        public string HighValue { get; set; }

        /// <summary>
        /// A list of values used in multi-value filters like IN and NOT_IN.
        /// </summary>
        public List<string> Values { get; set; }
    }
}