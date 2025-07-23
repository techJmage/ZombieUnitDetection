using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Detector.Parsing;

public partial class ParsedExpression
{
    public string Arg { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? LogicalOperator { get; set; } // "&&", "||", or null if it's the last/only condition

    public override string ToString() => $"{LogicalOperator} {Arg} {Operator} {Value}";

    public static List<ParsedExpression> Parse(string expr)
    {
        var parsedExprs = new List<ParsedExpression>();
        var matches = Parser().Matches(expr);
        string? previousLogicalOperator = null;
        foreach (Match match in matches)
        {
            var parsed = new ParsedExpression
            {
                Arg = match.Groups["Arg"].Value.Trim(),
                Operator = match.Groups["Operator"].Value.Trim(),
                Value = match.Groups["Value"].Value.Trim(),
                LogicalOperator = match.Groups["LogicalOperator"].Success ? match.Groups["LogicalOperator"].Value.Trim() : previousLogicalOperator
            };
            parsedExprs.Add(parsed);
            previousLogicalOperator = match.Groups["LogicalOperator"].Success ? match.Groups["LogicalOperator"].Value.Trim() : null;
        }
        return parsedExprs;
    }

    [GeneratedRegex(@"(?:(?<LogicalOperator>&&|\|\|)\s*)?(?<Arg>\w+)(?<Operator>(?:==|!=|<|>|<=|>=))(?<Value>[^&\|]+?)(?=$|&&|\|\|)")]
    private static partial Regex Parser();
}