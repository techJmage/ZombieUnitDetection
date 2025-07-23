using Detector.Parsing;

namespace Tests.Detector;

[TestClass]
public class ComparisionNodeTests
{
    [DataTestMethod]
    [DataRow("x>6 && x<9 || x>16 && x<19 || x<2 || x>22", "x>5 && x<10 || x>15 && x<20 || x<3 || x>21")]
    public void TestIsImpliedBy(string impliedExpr, string implyingExpr)
    {
        var impliedRoot = ComparisionNode.Parse(impliedExpr);
        var implyingRoot = ComparisionNode.Parse(implyingExpr);
        var isBeingImplied = impliedRoot.IsImpliedBy(implyingRoot);
        Assert.IsTrue(isBeingImplied);
    }
}
