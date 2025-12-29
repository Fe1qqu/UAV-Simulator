using UnityEngine;

public static class LevelValidator
{
    public static bool Validate(out string error)
    {
        if (Object.FindFirstObjectByType<DroneSpawnPoint>() == null)
        {
            error = "Level must contain a Drone Spawn Point.";
            return false;
        }

        error = null;
        return true;
    }
}
