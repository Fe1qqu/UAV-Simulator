using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEditor.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Preloads multiple localization tables in parallel.
/// </summary>
public class LocalizationPreloader : MonoBehaviour
{
    [Tooltip("Localization table collections to preload.")]
    public StringTableCollection[] stringTableCollections;

    private readonly Dictionary<StringTableCollection, AsyncOperationHandle<StringTable>> handles = new();

    public bool IsLoaded { get; private set; }

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

        foreach (StringTableCollection stringTableCollection in stringTableCollections)
        {
            if (stringTableCollection == null)
            {
                continue;
            }

            if (handles.ContainsKey(stringTableCollection))
            {
                continue;
            }

            var handle = LocalizationSettings.StringDatabase.GetTableAsync(stringTableCollection.TableCollectionName);
            handles.Add(stringTableCollection, handle);
            tasks.Add(handle.Task);
        }

        await Task.WhenAll(tasks);

        foreach (var kvp in handles)
        {
            var collection = kvp.Key;
            var handle = kvp.Value;

            if (!handle.IsValid() || handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[LocalizationPreloader] Failed to load table '{collection.TableCollectionName}'.");
                IsLoaded = false;
                return;
            }

            //Debug.Log($"[LocalizationPreloader] Loaded '{collection.TableCollectionName}'.");
        }

        IsLoaded = true;
        Debug.Log("[LocalizationPreloader] All tables loaded.");
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
        IsLoaded = false;
    }
}
