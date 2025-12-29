using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

/// <summary>
/// Preloads localization tables required by the Level Editor (Categories, PlaceableObjects).
/// </summary>
public class EditorLocalizationPreloader : MonoBehaviour
{
    [Tooltip("Name of categories StringTable.")]
    public string categoriesTableName = "Categories";

    [Tooltip("Name of placeable objects StringTable.")]
    public string objectsTableName = "PlaceableObjects";

    private AsyncOperationHandle<StringTable> categoriesHandle;
    private AsyncOperationHandle<StringTable> objectsHandle;

    private bool categoriesLoadedSuccessfully = false;
    private bool objectsLoadedSuccessfully = false;

    /// <summary>
    /// True when both tables are successfully loaded.
    /// </summary>
    public bool IsLoaded => categoriesLoadedSuccessfully && objectsLoadedSuccessfully;

    /// <summary>
    /// Loads both tables asynchronously.
    /// </summary>
    /// <summary>
    /// Loads both tables asynchronously in parallel.
    /// </summary>
    public async Task Load()
    {
        if (IsLoaded)
        {
            Debug.LogWarning("[EditorLocalizationPreloader] Tables already loaded.");
            return;
        }

        if (!categoriesLoadedSuccessfully)
        {
            categoriesHandle = LocalizationSettings.StringDatabase.GetTableAsync(categoriesTableName);
        }
        if (!objectsLoadedSuccessfully)
        {
            objectsHandle = LocalizationSettings.StringDatabase.GetTableAsync(objectsTableName);
        }

        // Await both tables in parallel
        await Task.WhenAll(
            categoriesHandle.IsValid() ? categoriesHandle.Task : Task.CompletedTask,
            objectsHandle.IsValid() ? objectsHandle.Task : Task.CompletedTask
        );

        // Check results
        if (categoriesHandle.IsValid() && categoriesHandle.Status == AsyncOperationStatus.Succeeded)
        {
            categoriesLoadedSuccessfully = true;
            Debug.Log($"[EditorLocalizationPreloader] Loaded '{categoriesTableName}'.");
        }
        else
        {
            categoriesLoadedSuccessfully = false;
            Debug.LogError($"[EditorLocalizationPreloader] Failed to load '{categoriesTableName}'.");
        }

        if (objectsHandle.IsValid() && objectsHandle.Status == AsyncOperationStatus.Succeeded)
        {
            objectsLoadedSuccessfully = true;
            Debug.Log($"[EditorLocalizationPreloader] Loaded '{objectsTableName}'.");
        }
        else
        {
            objectsLoadedSuccessfully = false;
            Debug.LogError($"[EditorLocalizationPreloader] Failed to load '{objectsTableName}'.");
        }
    }

    /// <summary>
    /// Unloads both tables and resets state.
    /// </summary>
    public void Unload()
    {
        if (categoriesHandle.IsValid())
        {
            Addressables.Release(categoriesHandle);
            categoriesHandle = default;
            categoriesLoadedSuccessfully = false;
        }

        if (objectsHandle.IsValid())
        {
            Addressables.Release(objectsHandle);
            objectsHandle = default;
            objectsLoadedSuccessfully = false;
        }

        Debug.Log("[EditorLocalizationPreloader] Tables unloaded.");
    }
}
