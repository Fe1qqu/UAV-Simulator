/*
NOTE:
LevelObjects in level editor are SOFT-DELETED, not destroyed.

- SoftDelete → LifecycleState.SoftDeleted + SetActive(false)
- Restore   → LifecycleState.Alive + SetActive(true)
- HardDelete happens ONLY on scene unload or cleanup

Because of this:
- Unregister is NOT called on level editor delete
- ObjectHierarchyList MUST react to LifecycleChanged
- LevelRuntimeRegistry always contains ALL objects (including deleted ones)
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ObjectHierarchyList : MonoBehaviour
{
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private LevelObjectRegistry levelObjectRegistry;
    [SerializeField] private LevelEditorDeleteController levelEditorDeleteController;
    [SerializeField] private SelectionManager selectionManager;

    // UI items
    private readonly Dictionary<LevelObject, UIObjectHierarchyItem> levelObjectToItemMap = new();
    private UIObjectHierarchyItem currentSelectedItem;

    // Optimization: SourcePlaceableObject cache → object list
    private readonly Dictionary<PlaceableObjectDefinition, List<LevelObject>> sourcePlaceableObjectToObjectsMap = new();

    private void Awake()
    {
        if (contentPanel == null)
        {
            Debug.LogError("[ObjectHierarchyList] ContentPanel is not assigned.");
        }

        if (scrollRect == null)
        {
            Debug.LogError("[ObjectHierarchyList] ScrollRect is not assigned.");
        }

        if (itemPrefab == null)
        {
            Debug.LogError("[ObjectHierarchyList] ItemPrefab is not assigned.");
        }

        if (levelObjectRegistry == null)
        {
            Debug.LogError("[ObjectHierarchyList] LevelObjectRegistry is not assigned.");
        }

        if (levelEditorDeleteController == null)
        {
            Debug.LogError("[ObjectHierarchyList] LevelEditorDeleteController is not assigned.");
        }

        if (selectionManager == null)
        {
            Debug.LogError("[ObjectHierarchyList] SelectionManager is not assigned.");
        }
    }

    private void OnEnable()
    {
        levelObjectRegistry.LevelObjectLifecycleChanged += OnLifecycleChanged;
        selectionManager.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        if (levelObjectRegistry != null)
        {
            levelObjectRegistry.LevelObjectLifecycleChanged -= OnLifecycleChanged;
        }

        if (selectionManager != null)
        {
            selectionManager.OnSelectionChanged -= OnSelectionChanged;
        }
    }

    private void Start()
    {
        foreach (LevelObject levelObject in levelObjectRegistry.LevelObjects)
        {
            GetOrCreateItem(levelObject);
            AddToSourceGroup(levelObject);
        }
    }

    private void OnLifecycleChanged(LevelObject levelObject)
    {
        if (!levelObjectToItemMap.TryGetValue(levelObject, out var item))
        {
            // Create UI only for Alive or SoftDeleted
            if (levelObject.LifecycleState != LevelObjectLifecycleState.HardDeleted)
            {
                item = GetOrCreateItem(levelObject);
            }
            else
            {
                // HardDeleted - do not create anything
                return;
            }
        }

        switch (levelObject.LifecycleState)
        {
            case LevelObjectLifecycleState.Alive:
                item.SetVisible(true);
                break;

            case LevelObjectLifecycleState.SoftDeleted:
                item.SetVisible(false);
                break;

            case LevelObjectLifecycleState.HardDeleted:
                if (item != null)
                {
                    levelObjectToItemMap.Remove(levelObject);
                    RemoveFromSourceGroup(levelObject);
                    Destroy(item.gameObject);
                }
                return; // There is no need to update displayName further
        }

        UpdateDisplayNamesForSourceGroup(levelObject.SourcePlaceableObject);
    }

    private void OnSelectionChanged(ISelectable selectable)
    {
        if (currentSelectedItem != null)
        {
            currentSelectedItem.SetSelected(false);
            currentSelectedItem = null;
        }

        if (selectable == null || selectable is not Component component)
        {
            return;
        }

        if (!component.TryGetComponent<LevelObject>(out var levelObject))
        {
            return;
        }

        if (!levelObjectToItemMap.TryGetValue(levelObject, out var item))
        {
            return;
        }

        item.SetSelected(true);
        currentSelectedItem = item;

        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        StartCoroutine(scrollRect.FocusOnItemCoroutine(item.GetComponent<RectTransform>(), 10f));
    }

    private UIObjectHierarchyItem GetOrCreateItem(LevelObject levelObject)
    {
        if (levelObjectToItemMap.TryGetValue(levelObject, out var existingItem))
        {
            return existingItem;
        }

        GameObject gameObject = Instantiate(itemPrefab, contentPanel);
        if (!gameObject.TryGetComponent(out UIObjectHierarchyItem item))
        {
            Debug.LogError("[ObjectHierarchyList] ItemPrefab missing UIObjectHierarchyItem component.");
            return null;
        }

        item.Bind(levelObject);
        item.OnDeleteRequested += OnDeleteRequested;
        item.OnSelectRequested += OnSelectRequested;

        levelObjectToItemMap[levelObject] = item;

        AddToSourceGroup(levelObject);

        return item;
    }

    private void OnDeleteRequested(LevelObject levelObject)
    {
        levelEditorDeleteController.DeleteObject(levelObject);
    }

    private void OnSelectRequested(LevelObject levelObject)
    {
        if (levelObject.TryGetComponent<ISelectable>(out var selectable))
        {
            selectionManager.Select(selectable);
        }
    }

    private void AddToSourceGroup(LevelObject levelObject)
    {
        if (levelObject.SourcePlaceableObject == null)
        {
            return;
        }

        if (!sourcePlaceableObjectToObjectsMap.TryGetValue(levelObject.SourcePlaceableObject, out var levelObjectsList))
        {
            levelObjectsList = new List<LevelObject>();
            sourcePlaceableObjectToObjectsMap[levelObject.SourcePlaceableObject] = levelObjectsList;
        }

        if (!levelObjectsList.Contains(levelObject))
        {
            levelObjectsList.Add(levelObject);
        }
    }

    private void RemoveFromSourceGroup(LevelObject levelObject)
    {
        if (levelObject.SourcePlaceableObject == null)
        {
            return;
        }

        if (sourcePlaceableObjectToObjectsMap.TryGetValue(levelObject.SourcePlaceableObject, out var levelObjectsList))
        {
            levelObjectsList.Remove(levelObject);
            if (levelObjectsList.Count == 0)
            {
                sourcePlaceableObjectToObjectsMap.Remove(levelObject.SourcePlaceableObject);
            }
        }
    }

    private void UpdateDisplayNamesForSourceGroup(PlaceableObjectDefinition sourcePlaceableObject)
    {
        if (sourcePlaceableObject == null || !sourcePlaceableObjectToObjectsMap.TryGetValue(sourcePlaceableObject, out var group))
        {
            return;
        }

        int index = 1;

        foreach (LevelObject levelObject in group)
        {
            if (!levelObject.IsAlive)
            {
                continue;
            }

            if (!levelObjectToItemMap.TryGetValue(levelObject, out var item))
            {
                continue;
            }

            string baseName = levelObject.SourcePlaceableObject.localizedString.GetLocalizedString();
            string displayName = index == 1 ? baseName : $"{baseName} ({index})";
            item.SetDisplayName(displayName);

            index++;
        }
    }
}
