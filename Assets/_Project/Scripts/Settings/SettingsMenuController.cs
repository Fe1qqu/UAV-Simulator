using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Universal SettingsMenu controller.
/// </summary>
public class SettingsMenuController : MonoBehaviour
{
    [Header("UI references")]
    [Tooltip("Container for tab buttons.")]
    [SerializeField] private RectTransform tabsContainer;

    [Tooltip("Tab button prefab.")]
    [SerializeField] private GameObject tabButtonPrefab;

    [Tooltip("Container with pages - child objects must have SettingsPage components.")]
    [SerializeField] private RectTransform pagesContainer;

    //[Tooltip("Close button.")]
    //[SerializeField] private Button closeButton;

    [Header("Context -> page ids mapping")]
    [Tooltip("Matches a context with a set of pageIds that should be shown in that context..")]
    [SerializeField] private List<ContextTabMapping> contextMappings = new List<ContextTabMapping>();

    private Dictionary<string, SettingsPage> pagesById = new Dictionary<string, SettingsPage>();
    private List<TabButton> createdTabButtons = new List<TabButton>();
    private SettingsPage activePage;
    private SettingsContext? currentContext = null;

    private void Awake()
    {
        if (tabsContainer == null)
        {
            Debug.LogError("[SettingsMenuController] TabsContainer not assigned.");
        }

        if (tabButtonPrefab == null)
        {
            Debug.LogError("[SettingsMenuController] TabButtonPrefab not assigned.");
        }

        if (pagesContainer == null)
        {
            Debug.LogError("[SettingsMenuController] PagesContainer not assigned.");
        }

        //if (closeButton == null)
        //{
        //    Debug.LogError("[SettingsMenuController] CloseButton not assigned.");
        //}

        CachePages();
    }

    private void CachePages()
    {
        pagesById.Clear();
        foreach (var page in pagesContainer.GetComponentsInChildren<SettingsPage>(true))
        {
            if (string.IsNullOrEmpty(page.PageId))
            {
                Debug.LogWarning($"[SettingsMenuController] Page '{page.name}' has empty PageId.");
                continue;
            }

            if (pagesById.ContainsKey(page.PageId))
            {
                Debug.LogWarning($"[SettingsMenuController] Duplicate PageId '{page.PageId}' found in '{page.name}'.");
                continue;
            }

            pagesById.Add(page.PageId, page);
            page.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the settings menu for the specified context.
    /// </summary>
    public void Show(SettingsContext context)
    {
        gameObject.SetActive(true);

        if (currentContext == context)
        {
            return;
        }

        currentContext = context;
        BuildTabsForContext(context);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Removes old tabs and creates new ones according to the context mapping.
    /// </summary>
    private void BuildTabsForContext(SettingsContext context)
    {
        foreach (var button in createdTabButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        createdTabButtons.Clear();
        activePage = null;

        // Find mapping
        var mapping = contextMappings.Find(m => m.context == context);
        if (mapping == null || mapping.pageIds == null || mapping.pageIds.Count == 0)
        {
            Debug.LogWarning($"[SettingsMenuController] No tabs configured for context {context}");
            return;
        }

        // Create tab buttons in order
        foreach (var pageId in mapping.pageIds)
        {
            if (!pagesById.TryGetValue(pageId, out var page))
            {
                Debug.LogWarning($"[SettingsMenuController] PageId '{pageId}' not found in pagesContainer.");
                continue;
            }

            var tabButtonInstance = Instantiate(tabButtonPrefab, tabsContainer, false);
            if (!tabButtonInstance.TryGetComponent<TabButton>(out var tabButtonComponent))
            {
                Debug.LogError("[SettingsMenuController] TabButtonPrefab missing TabButton component.");
                Destroy(tabButtonInstance);
                continue;
            }

            tabButtonComponent.Setup(page.PageId, page.DisplayName, this);
            createdTabButtons.Add(tabButtonComponent);
        }

        string firstPageId = mapping.pageIds.FirstOrDefault(id => pagesById.ContainsKey(id));
        if (!string.IsNullOrEmpty(firstPageId))
        {
            SelectPage(firstPageId);
        }
    }

    /// <summary>
    /// Called when a tab is clicked. Shows the page with the specified pageId.
    /// </summary>
    public void SelectPage(string pageId)
    {
        if (string.IsNullOrEmpty(pageId))
        {
            return;
        }

        if (!pagesById.TryGetValue(pageId, out var page))
        {
            Debug.LogWarning($"[SettingsMenuController] SelectPage: pageId '{pageId}' not found.");
            return;
        }

        if (activePage == page)
        {
            return;
        }

        // Unselect old
        if (activePage != null)
        {
            activePage.OnPageUnselected();
            activePage.gameObject.SetActive(false);
        }

        // Activate new
        activePage = page;
        activePage.gameObject.SetActive(true);
        activePage.OnPageSelected();

        foreach (var tab in createdTabButtons)
        {
            bool isSelected = tab.PageId == pageId;
            tab.SetSelected(isSelected);
        }

        Debug.Log($"[SettingsMenuController] Selected page '{pageId}'");
    }
}
