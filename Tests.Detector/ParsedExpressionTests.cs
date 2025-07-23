using Detector.Parsing;

namespace Tests.Detector;

[TestClass]
public class ParsedExpressionTests
{
    [DataTestMethod]
    [DataRow("x>10")]
    [DataRow("x>5 && x<10")]
    [DataRow("x>5 && x<10 || x>15 && x<20")]
    [DataRow("x == \"RED\"")]
    [DataRow("x == \"RED\" || x == \"BLUE\"")]
    [DataRow("x != \"RED\"")]
    [DataRow("x != \"RED\" || x != \"BLUE\"")]
    [DataRow("x == true")]
    [DataRow("x == false")]
    [DataRow("x != true")]
    [DataRow("x != false")]
    public void TestParse(string expr)
    {
        var parsed = ParsedExpression.Parse(expr);
        Assert.IsNotNull(parsed);
        Assert.IsTrue(parsed.Count > 0);
        parsed.ForEach(Console.WriteLine);
    }
}
