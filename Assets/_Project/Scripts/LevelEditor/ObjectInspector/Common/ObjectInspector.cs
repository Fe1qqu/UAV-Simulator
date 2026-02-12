using UnityEngine;

public enum ObjectInspectorTab
{
    None,
    Transform,
    Properties
}

public class ObjectInspector : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Tabs")]
    [SerializeField] private TransformInspectorTab transformInspectorTab;
    [SerializeField] private PropertiesInspectorTab propertiesInspectorTab;

    [Header("Tab Buttons")]
    [SerializeField] private InspectorTabButton transformInspectorTabButton;
    [SerializeField] private InspectorTabButton propertiesInspectorTabButton;

    [SerializeField] private SelectionManager selectionManager;

    private LevelObject currentObject;
    private ObjectInspectorTab activeTab = ObjectInspectorTab.None;

    private void Awake()
    {
        if (root == null)
        {
            Debug.LogError("[ObjectInspector] Root is not assigned.");
        }

        if (transformInspectorTab == null)
        {
            Debug.LogError("[ObjectInspector] TransformInspectorTab is not assigned.");
        }

        if (propertiesInspectorTab == null)
        {
            Debug.LogError("[ObjectInspector] PropertiesInspectorTab is not assigned.");
        }

        if (transformInspectorTabButton == null)
        {
            Debug.LogError("[ObjectInspector] TransformInspectorTabButton is not assigned.");
        }

        if (propertiesInspectorTabButton == null)
        {
            Debug.LogError("[ObjectInspector] PropertiesInspectorTabButton is not assigned.");
        }

        if (selectionManager == null)
        {
            Debug.LogError("[ObjectInspector] SelectionManager is not assigned.");
        }

        transformInspectorTabButton.Initialize(this);
        propertiesInspectorTabButton.Initialize(this);

        root.SetActive(false);
    }

    private void OnEnable()
    {
        selectionManager.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        if (selectionManager != null)
        {
            selectionManager.OnSelectionChanged -= OnSelectionChanged;
        }
    }

    private void OnSelectionChanged(ISelectable selectable)
    {
        if (selectable == null || selectable is not Component component)
        {
            Clear();
            return;
        }

        if (!component.TryGetComponent<LevelObject>(out var levelObject))
        {
            Clear();
            return;
        }

        Bind(levelObject);
    }

    private void Bind(LevelObject levelObject)
    {
        currentObject = levelObject;
        root.SetActive(true);

        transformInspectorTab.Bind(levelObject);
        propertiesInspectorTab.Bind(levelObject);

        SelectTab(ObjectInspectorTab.Transform);
    }

    private void Clear()
    {
        currentObject = null;
        root.SetActive(false);

        activeTab = ObjectInspectorTab.None;

        transformInspectorTab.Clear();
        propertiesInspectorTab.Clear();
    }

    public void SelectTab(ObjectInspectorTab tab)
    {
        if (activeTab == tab)
        {
            return;
        }

        activeTab = tab;

        transformInspectorTab.gameObject.SetActive(tab == ObjectInspectorTab.Transform);
        propertiesInspectorTab.gameObject.SetActive(tab == ObjectInspectorTab.Properties);

        transformInspectorTabButton.SetSelected(tab == ObjectInspectorTab.Transform);
        propertiesInspectorTabButton.SetSelected(tab == ObjectInspectorTab.Properties);

        if (tab == ObjectInspectorTab.Transform)
        {
            transformInspectorTab.OnActivated();
            propertiesInspectorTab.OnDeactivated();
        }
        else
        {
            propertiesInspectorTab.OnActivated();
            transformInspectorTab.OnDeactivated();
        }
    }
}
