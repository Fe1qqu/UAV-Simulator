using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Preloads multiple localization tables in parallel.
/// </summary>
public class LocalizationPreloader : MonoBehaviour
{
    [Tooltip("List of table names to preload.")]
    public string[] tableNames;

    private Dictionary<string, AsyncOperationHandle<StringTable>> handles = new();
    private Dictionary<string, bool> loadedFlags = new();

    /// <summary>
    /// True if all tables are successfully loaded.
    /// </summary>
    public bool IsLoaded
    {
        get
        {
            foreach (string name in tableNames)
            {
                if (!loadedFlags.TryGetValue(name, out bool loaded) || !loaded)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Loads all tables in parallel.
    /// </summary>
    public async Task Load()
    {
        if (IsLoaded)
        {
            Debug.LogWarning("[LocalizationPreloader] Tables already loaded.");
            return;
        }

        List<Task> tasks = new();

        foreach (string tableName in tableNames)
        {
            if (loadedFlags.TryGetValue(tableName, out bool loaded) && loaded)
            {
                continue;
            }

            var handle = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
            handles[tableName] = handle;
            tasks.Add(handle.Task);
        }

        await Task.WhenAll(tasks);

        foreach (var kvp in handles)
        {
            string name = kvp.Key;
            var handle = kvp.Value;

            if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedFlags[name] = true;
                Debug.Log($"[LocalizationPreloader] Loaded '{name}'.");
            }
            else
            {
                loadedFlags[name] = false;
                Debug.LogError($"[LocalizationPreloader] Failed to load '{name}'.");
            }
        }
    }

    /// <summary>
    /// Unloads all tables and resets state.
    /// </summary>
    public void Unload()
    {
        foreach (var kvp in handles)
        {
            if (kvp.Value.IsValid())
            {
                Addressables.Release(kvp.Value);
            }
        }

        handles.Clear();
        loadedFlags.Clear();

        Debug.Log("[LocalizationPreloader] All tables unloaded.");
    }
}
