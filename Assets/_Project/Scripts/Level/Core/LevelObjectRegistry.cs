/*
NOTE:
In the level editor, LevelObjects are NOT destroyed on delete.
Instead, a soft-delete approach is used via SetActive(false) to support undo/redo.

Because of this:
- LevelObject.Unregister() is NOT called on delete
- LevelObjectRemoved event is NOT fired during normal editor workflow
- LevelObjectRemoved is only triggered when the object is actually destroyed
  (e.g. scene unload, domain reload, hard cleanup)

UI systems (e.g. ObjectHierarchyList) must react to
LevelObjectStateChanged instead of LevelObjectRemoved.
*/

using UnityEngine;
using System;
using System.Collections.Generic;

public class LevelObjectRegistry : MonoBehaviour
{
    public static LevelObjectRegistry Instance { get; private set; }

    public event Action<LevelObject> LevelObjectRegistered;
    public event Action<LevelObject> LevelObjectUnregistered;
    public event Action<LevelObject> LevelObjectLifecycleChanged;

    private readonly List<LevelObject> levelObjects = new();
    public IReadOnlyList<LevelObject> LevelObjects => levelObjects;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("[LevelObjectRegistry] Duplicate instance detected. Only one instance is allowed in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Register(LevelObject levelObject)
    {
        if (levelObjects.Contains(levelObject))
        {
            return;
        }

        levelObjects.Add(levelObject);
        LevelObjectRegistered?.Invoke(levelObject);
    }

    public void Unregister(LevelObject levelObject)
    {
        if (!levelObjects.Remove(levelObject))
        {
            return;
        }

        LevelObjectUnregistered?.Invoke(levelObject);
    }

    public void NotifyLifecycleChanged(LevelObject levelObject)
    {
        LevelObjectLifecycleChanged?.Invoke(levelObject);
    }

    public T FindFirstAlive<T>() where T : LevelObject
    {
        foreach (LevelObject levelObject in levelObjects)
        {
            if (!levelObject.IsAlive)
            {
                continue;
            }

            if (levelObject is T typed)
            {
                return typed;
            }
        }

        return null;
    }

    public List<T> FindAllAlive<T>() where T : LevelObject
    {
        List<T> result = new();

        foreach (LevelObject levelObject in levelObjects)
        {
            if (!levelObject.IsAlive)
            {
                continue;
            }

            if (levelObject is T typed)
            {
                result.Add(typed);
            }
        }

        return result;
    }

    public IEnumerable<T> EnumerateAlive<T>() where T : LevelObject
    {
        foreach (LevelObject levelObject in levelObjects)
        {
            if (!levelObject.IsAlive)
            {
                continue;
            }

            if (levelObject is T typed)
            {
                yield return typed;
            }    
        }
    }
}
