using Detector.Model;

static void Print(List<Unit> units)
{
    int unitCount = units.Count;
    if (unitCount == 0)
    {
        Console.WriteLine("No units to display.");
        return;
    }

    // Collect all unique arguments
    var allArgs = units.SelectMany(u => u.Causes.Select(c => c.Arg)).Distinct().OrderBy(a => a).ToList();

    // Calculate column widths
    int argWidth = 5;
    int expressionWidth = 18;
    int columnWidth = argWidth + 3 + expressionWidth; // arg : expression

    // Print header
    Console.Write(" ");
    for (int i = 0; i < unitCount; i++)
    {
        Console.Write($"{"R" + units[i].Sequence:D2}".PadLeft(columnWidth));
    }
    Console.WriteLine();

    // Print separator
    Console.Write(" ");
    for (int i = 0; i < unitCount; i++)
    {
        Console.Write(new string('-', columnWidth));
    }
    Console.WriteLine();

    // Print data rows
    foreach (var arg in allArgs)
    {
        Console.Write($" {arg.PadRight(argWidth)} : ");
        for (int i = 0; i < unitCount; i++)
        {
            var cause = units[i].Causes.FirstOrDefault(c => c.Arg == arg);
            Console.Write($"{(cause?.Expression ?? "").PadLeft(expressionWidth)}  ");
        }
        Console.WriteLine();
    }
}

Console.WriteLine("All rules");
List<Unit> units =
[
    new Unit { Sequence = 1, Causes = [new Cause { Arg = "x", Expression = "x<10" }, new Cause { Arg = "y", Expression = "y<15" }] },
    new Unit { Sequence = 2, Causes = [new Cause { Arg = "x", Expression = "x<5" }, new Cause { Arg = "y", Expression = "y<10" }] },
    new Unit { Sequence = 3, Causes = [new Cause { Arg = "x", Expression = "x<20" }, new Cause { Arg = "y", Expression = "y<25" }] },
    new Unit { Sequence = 4, Causes = [new Cause { Arg = "x", Expression = "x>5 && x<8" }] },
    new Unit { Sequence = 5, Causes = [new Cause { Arg = "x", Expression = "x>5 && x<10 || x>15 && x<20 || x<3 || x>21" }] },
    new Unit { Sequence = 6, Causes = [new Cause { Arg = "x", Expression = "x>6 && x<9 || x>16 && x<19 || x<2 || x>22" }] },
    new Unit { Sequence = 7, Causes = [new Cause { Arg = "x", Expression = "x>4 && x<9" }] }
];
Print(units);

Console.WriteLine("\nZombie rules");
var redundantUnits = Detector.Detection.ZombieDetector.Do(units);
Print(redundantUnits);
