namespace Detector.Parsing;
using Range = (int? Lower, int? Upper);

public abstract class ComparisionNode
{
    public string Arg { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;

    public abstract bool IsImpliedBy(ComparisionNode other);
    public static ComparisionNode? Parse(string expression)
    {
        var parsedExprs = ParsedExpression.Parse(expression);
        if (parsedExprs.Count == 0)
            return null;
        List<ComparisionNode?> nodes = [];
        List<string> operators = [];
        foreach (var parsed in parsedExprs)
        {
            nodes.Add(Parse(parsed));
            if (parsed.LogicalOperator != null)
                operators.Add(parsed.LogicalOperator);
        }
        return BuildExpressionTree(nodes, operators);

        static Range ParseInteger(ParsedExpression parsed)
        {
            int? lower = null;
            int? upper = null;
            if (int.TryParse(parsed.Value, out int value))
            {
                switch (parsed.Operator)
                {
                    case "<": upper = value - 1; break;
                    case "<=": upper = value; break;
                    case ">": lower = value + 1; break;
                    case ">=": lower = value; break;
                    case "==": lower = upper = value; break;
                    case "!=": break; // Handle != later
                }
            }
            return (lower, upper);
        }
        static List<string> ParseString(ParsedExpression parsed) => [parsed.Value.Trim('"')];
        static List<bool> ParseBoolean(ParsedExpression parsed) => [bool.Parse(parsed.Value)];
        static ComparisionNode? Parse(ParsedExpression parsed)
        {
            if (int.TryParse(parsed.Value, out int intValue) && parsed.Operator == "!=")
            {
                var lessThanNode = new ComparisionLeaf
                {
                    Comparision = ParseInteger(new ParsedExpression { Arg = parsed.Arg, Operator = "<", Value = parsed.Value }),
                    Arg = parsed.Arg,
                    Operator = "<"
                };
                var greaterThanNode = new ComparisionLeaf
                {
                    Comparision = ParseInteger(new ParsedExpression { Arg = parsed.Arg, Operator = ">", Value = parsed.Value }),
                    Arg = parsed.Arg,
                    Operator = ">"
                };
                return new OperatorNode { Operator = "||", Left = lessThanNode, Right = greaterThanNode };
            }
            else if ((parsed.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                    parsed.Value.Equals("false", StringComparison.OrdinalIgnoreCase)) &&
                    parsed.Operator == "!=")
            {
                string oppositeValue = parsed.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ? "false" : "true";
                return new ComparisionLeaf
                {
                    Comparision = ParseBoolean(new ParsedExpression { Arg = parsed.Arg, Operator = "==", Value = oppositeValue }),
                    Arg = parsed.Arg,
                    Operator = "=="
                };
            }
            else
            {
                object condition = null;
                if (int.TryParse(parsed.Value, out _))
                    condition = ParseInteger(parsed);
                else if (parsed.Value.Equals("true", StringComparison.OrdinalIgnoreCase) || parsed.Value.Equals("false", StringComparison.OrdinalIgnoreCase))
                    condition = ParseBoolean(parsed);
                else if (parsed.Operator == "==" && (parsed.Value.StartsWith('\"') && parsed.Value.EndsWith('\"')))
                    condition = ParseString(parsed);
                else if (parsed.Operator == "!=" && (parsed.Value.StartsWith('\"') && parsed.Value.EndsWith('\"')))
                    condition = ParseString(parsed);
                else
                    return null;
                return new ComparisionLeaf { Comparision = condition, Arg = parsed.Arg, Operator = parsed.Operator };
            }
        }
        static ComparisionNode? BuildExpressionTree(List<ComparisionNode?> nodes, List<string> operators)
        {
            // Handle &&m
            int index = operators.IndexOf("&&");
            while (index != -1)
            {
                nodes[index] = new OperatorNode { Arg = nodes[index].Arg, Operator = "&&", Left = nodes[index], Right = nodes[index + 1] };
                nodes.RemoveAt(index + 1);
                operators.RemoveAt(index);
                index = operators.IndexOf("&&");
            }
            // Handle ||
            if (nodes.Count > 1)
            {
                ComparisionNode? root = nodes[0];
                for (int i = 0; i < operators.Count; i++)
                    root = new OperatorNode { Arg = root.Arg, Operator = "||", Left = root, Right = nodes[i + 1] };
                return root;
            }
            return nodes.FirstOrDefault();
        }
    }
}

public class ComparisionLeaf : ComparisionNode
{
    public object? Comparision { get; set; } = default;

    public override bool IsImpliedBy(ComparisionNode other)
    {
        if (other is ComparisionLeaf otherLeaf && Arg == otherLeaf.Arg)
            return Implies(otherLeaf.Comparision, Comparision, otherLeaf.Operator, Operator);
        if (other is OperatorNode otherOperator && Arg == otherOperator.Left?.Arg && Arg == otherOperator.Right?.Arg)
            return ImpliedByOperatorNode(otherOperator, this);
        return false;
    }
    private static bool Implies(object conditionA, object conditionB, string operatorA, string operatorB)
    {
        if (conditionA is Range former && conditionB is Range later)
        {
            if (later.Lower != null && (former.Lower == null || former.Lower > later.Lower))
                return false;
            if (later.Upper != null && (former.Upper == null || former.Upper < later.Upper))
                return false;
            return true;
        }
        if (conditionA is List<string> valuesA && conditionB is List<string> valuesB)
        {
            if (operatorA == "==" && operatorB == "==")
                return valuesA.TrueForAll(valuesB.Contains);
            if (operatorA == "!=" && operatorB == "!=")
                return valuesA.SequenceEqual(valuesB);
            if (operatorA == "==" && operatorB == "!=")
                return !valuesA.Exists(valuesB.Contains);
            if (operatorA == "!=" && operatorB == "==")
                return !valuesB.Exists(valuesA.Contains);
            return false;
        }
        if (conditionA is List<bool> boolsA && conditionB is List<bool> boolsB)
            return boolsA.TrueForAll(boolsB.Contains);
        return false;
    }
    private static bool ImpliedByOperatorNode(OperatorNode implyingNode, ComparisionLeaf impliedLeaf)
    {
        if (implyingNode == null)
            return false;
        if (implyingNode.Operator == "&&")
            // The leaf must be implied by BOTH sides
            return ImpliedByOperand(implyingNode.Left, impliedLeaf) && ImpliedByOperand(implyingNode.Right, impliedLeaf);
        if (implyingNode.Operator == "||")
            // The leaf must be implied by AT LEAST ONE side
            return ImpliedByOperand(implyingNode.Left, impliedLeaf) || ImpliedByOperand(implyingNode.Right, impliedLeaf);
        return false;

        static bool ImpliedByOperand(ComparisionNode operand, ComparisionLeaf impliedLeaf)
        {
            if (operand is ComparisionLeaf operandLeaf && operandLeaf.Arg == impliedLeaf.Arg)
                // Direct leaf to leaf implication
                return impliedLeaf.IsImpliedBy(operandLeaf);
            if (operand is OperatorNode operandOperator)
                // Recurse down the operator node
                return ImpliedByOperatorNode(operandOperator, impliedLeaf);
            return false;
        }
    }
}

public class OperatorNode : ComparisionNode
{
    public ComparisionNode? Left { get; set; }
    public ComparisionNode? Right { get; set; }

    public override bool IsImpliedBy(ComparisionNode other)
    {
        if (other is OperatorNode operatorOther)
        {
            if (Operator == "&&")
                return Left.IsImpliedBy(operatorOther) && Right.IsImpliedBy(operatorOther);
            if (Operator == "||")
                return Left.IsImpliedBy(operatorOther) || Right.IsImpliedBy(operatorOther);
        }
        return false;
    }
}
