using System.Collections.Generic;
using System.Linq;

public static class CheckpointSequenceService
{
    public static List<Checkpoint> GetOrdered(IEnumerable<Checkpoint> checkpoints)
    {
        return checkpoints
            .Where(c => c != null)
            .OrderBy(c => c.Get(LevelPropertyKeys.Index, int.MaxValue))
            .ThenBy(c => c.GetEntityId())
            .ToList();
    }

    public static bool IsContinuousSequence(List<Checkpoint> ordered)
    {
        for (int i = 0; i < ordered.Count; i++)
        {
            int index = ordered[i].Get(LevelPropertyKeys.Index, -1);
            if (index != i)
            {
                return false;
            }
        }

        return true;
    }

    public static bool TryGetValidSequence(IEnumerable<Checkpoint> checkpoints,out List<Checkpoint> ordered)
    {
        ordered = GetOrdered(checkpoints);

        return IsContinuousSequence(ordered);
    }
}
