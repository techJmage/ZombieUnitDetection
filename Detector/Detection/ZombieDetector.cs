using Detector.Model;
using Detector.Parsing;

namespace Detector.Detection;

public static class ZombieDetector
{
    public static List<Unit> Do(List<Unit> units)
    {
        List<Unit> redundantUnits = [];
        var sortedUnits = units.OrderBy(u => u.Sequence).ToList();
        for (int i = 1; i < sortedUnits.Count; i++)
        {
            Unit target = sortedUnits[i];
            for (int j = 0; j < i; j++)
            {
                Unit potentialImplier = sortedUnits[j];
                if (Implies(potentialImplier, target))
                {
                    redundantUnits.Add(target);
                    break; // Found a unit with lower sequence that implies unitB, so it's redundant
                }
            }
        }
        return redundantUnits;
    }
    private static bool Implies(Unit potentialImplier, Unit target)
    {
        if (target.Causes.Count == 0)
            return true; // An empty Unit is implied by any Unit
        if (potentialImplier.Causes.Count == 0 && target.Causes.Count > 0)
            return false; // An empty Unit cannot imply a non-empty Unit
        foreach (var implied in target.Causes)
        {
            bool isImplied = false;
            foreach (var implying in potentialImplier.Causes)
            {
                var impliedRoot = ComparisionNode.Parse(implied.Expression);
                var implyingRoot = ComparisionNode.Parse(implying.Expression);

                if (implying.Arg == implied.Arg && IsImpliedBy(impliedRoot, implyingRoot))
                {
                    isImplied = true;
                    break;
                }
            }
            if (!isImplied)
                return false;
        }
        return true;
    }
    private static bool IsImpliedBy(ComparisionNode? implied, ComparisionNode? implying)
    {
        if (implying == null)
            return true;
        if (implied == null)
            return false;
        if (implied.Arg != implying.Arg)
            return false;
        return implied.IsImpliedBy(implying);
    }
}
