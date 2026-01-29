/*
NOTE:
LevelObjects in editor are SOFT-DELETED, not destroyed.

- SoftDelete → LifecycleState.SoftDeleted + SetActive(false)
- Restore   → LifecycleState.Alive + SetActive(true)
- HardDelete happens ONLY on scene unload or cleanup

Because of this:
- Unregister is NOT called on editor delete
- ObjectHierarchyList MUST react to LifecycleChanged
- LevelRuntimeRegistry always contains ALL objects (including deleted ones)
*/

using UnityEngine;
using System.Collections.Generic;

public class ObjectHierarchyList : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private LevelRuntimeRegistry levelRuntimeRegistry;
    [SerializeField] private EditorDeleteController editorDeleteController;
    [SerializeField] private SelectionManager selectionManager;

    private readonly Dictionary<LevelObject, UIObjectHierarchyItem> itemsMap = new();

    private void Awake()
    {
        if (contentRoot == null)
        {
            Debug.LogError("[ObjectHierarchyList] ContentRoot is not assigned.");
        }

        if (itemPrefab == null)
        {
            Debug.LogError("[ObjectHierarchyList] ItemPrefab is not assigned.");
        }

        if (levelRuntimeRegistry == null)
        {
            Debug.LogError("[ObjectHierarchyList] LevelRuntimeRegistry is not assigned.");
        }

        if (editorDeleteController == null)
        {
            Debug.LogError("[ObjectHierarchyList] EditorDeleteController is not assigned.");
        }

        if (selectionManager == null)
        {
            Debug.LogError("[ObjectHierarchyList] SelectionManager is not assigned.");
        }
    }

    private void OnEnable()
    {
        levelRuntimeRegistry.LevelObjectLifecycleChanged += OnLifecycleChanged;
    }

    private void OnDisable()
    {
        if (levelRuntimeRegistry == null)
        {
            return;
        }

        levelRuntimeRegistry.LevelObjectLifecycleChanged -= OnLifecycleChanged;
    }

    private void Start()
    {
        RefreshAll();
    }

    private void RefreshAll()
    {
        foreach (UIObjectHierarchyItem item in itemsMap.Values)
        {
            Destroy(item.gameObject);
        }
        itemsMap.Clear();

        foreach (LevelObject levelObject in levelRuntimeRegistry.LevelObjects)
        {
            OnLifecycleChanged(levelObject);
        }
    }

    private void OnLifecycleChanged(LevelObject levelObject)
    {
        switch (levelObject.LifecycleState)
        {
            case LevelObjectLifecycleState.Alive:
                AddItem(levelObject);
                break;

            case LevelObjectLifecycleState.SoftDeleted:
                RemoveItem(levelObject);
                break;

            case LevelObjectLifecycleState.HardDeleted:
                RemoveItem(levelObject);
                break;
        }
    }

    private void AddItem(LevelObject levelObject)
    {
        if (itemsMap.ContainsKey(levelObject))
        {
            return;
        }

        GameObject itemGameObject = Instantiate(itemPrefab, contentRoot);
        if (!itemGameObject.TryGetComponent<UIObjectHierarchyItem>(out var item))
        {
            Debug.LogError("[ObjectHierarchyList] ItemPrefab missing UIObjectHierarchyItem component.");
            return;
        }

        itemGameObject.SetActive(true);
        item.Bind(levelObject);
        item.OnDeleteRequested += OnDeleteRequested;
        item.OnSelectRequested += OnSelectRequested;

        itemsMap[levelObject] = item;
    }

    private void RemoveItem(LevelObject levelObject)
    {
        if (!itemsMap.TryGetValue(levelObject, out var item))
        {
            return;
        }

        if (item != null)
        {
            item.Unbind();
            Destroy(item.gameObject);
        }

        itemsMap.Remove(levelObject);
    }

    private void OnDeleteRequested(LevelObject levelObject)
    {
        editorDeleteController.DeleteObject(levelObject);
    }

    private void OnSelectRequested(LevelObject levelObject)
    {
        selectionManager.SelectObject(levelObject.GetComponent<SelectableObject>());
    }
}
