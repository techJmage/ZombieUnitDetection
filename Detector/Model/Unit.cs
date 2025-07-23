namespace Detector.Model;

public class Unit
{
    public int Sequence { get; set; }
    public List<Cause> Causes { get; set; } = [];
    public override string ToString()
    {
        var header = $"R{Sequence:00}";
        var causes = string.Join("\n", Causes.Select(c => $"{c.Arg}: {c.Expression}"));
        return header + "\n" + causes;
    }
}
