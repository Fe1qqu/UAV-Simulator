using UnityEngine;
using UnityEditor.Localization;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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
    public async Task Load(CancellationToken cancellationToken = default)
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

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var handle = LocalizationSettings.StringDatabase.GetTableAsync(stringTableCollection.TableCollectionName);
            handles.Add(stringTableCollection, handle);
            tasks.Add(handle.Task);
        }

        await Task.WhenAll(tasks);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        foreach (var keyValuePair in handles)
        {
            var collection = keyValuePair.Key;
            var handle = keyValuePair.Value;

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
        foreach (var keyValuePair in handles)
        {
            if (keyValuePair.Value.IsValid())
            {
                Addressables.Release(keyValuePair.Value);
            }
        }

        handles.Clear();
        IsLoaded = false;
    }
}
