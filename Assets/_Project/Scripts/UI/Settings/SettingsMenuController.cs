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
    [SerializeField] private RectTransform tabsButtonsContainer;

    [Tooltip("Tab button prefab.")]
    [SerializeField] private GameObject tabButtonPrefab;

    [Tooltip("Container with tabs - child objects must have SettingsTab components.")]
    [SerializeField] private RectTransform tabsContainer;

    //[Tooltip("Close button.")]
    //[SerializeField] private Button closeButton;

    [Header("Context -> tab ids mapping")]
    [Tooltip("Matches a context with a set of tabIds that should be shown in that context.")]
    [SerializeField] private List<SettingsContextTabs> settingsContextTabs = new List<SettingsContextTabs>();

    private Dictionary<string, SettingsTab> tabsById = new Dictionary<string, SettingsTab>();
    private List<TabButton> createdTabButtons = new List<TabButton>();
    private SettingsTab activeTab;
    private SettingsContext? currentContext = null;

    private void Awake()
    {
        if (tabsButtonsContainer == null)
        {
            Debug.LogError("[SettingsMenuController] TabsButtonsContainer not assigned.");
        }

        if (tabButtonPrefab == null)
        {
            Debug.LogError("[SettingsMenuController] TabButtonPrefab not assigned.");
        }

        if (tabsContainer == null)
        {
            Debug.LogError("[SettingsMenuController] TabsContainer not assigned.");
        }

        CacheTabs();
    }

    private void CacheTabs()
    {
        tabsById.Clear();
        foreach (SettingsTab tab in tabsContainer.GetComponentsInChildren<SettingsTab>(true))
        {
            if (string.IsNullOrEmpty(tab.TabId))
            {
                Debug.LogWarning($"[SettingsMenuController] Tab '{tab.name}' has empty Id.");
                continue;
            }

            if (tabsById.ContainsKey(tab.TabId))
            {
                Debug.LogWarning($"[SettingsMenuController] Duplicate Id '{tab.TabId}' found in '{tab.name}'.");
                continue;
            }

            tabsById.Add(tab.TabId, tab);
            tab.gameObject.SetActive(false);
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
        activeTab = null;

        SettingsContextTabs contextTabs = settingsContextTabs.Find(c => c.context == context);
        if (contextTabs == null || contextTabs.tabIds == null || contextTabs.tabIds.Count == 0)
        {
            Debug.LogWarning($"[SettingsMenuController] No tabs configured for context {context}");
            return;
        }

        // Create tab buttons in order
        foreach (string tabId in contextTabs.tabIds)
        {
            if (!tabsById.TryGetValue(tabId, out var tab))
            {
                Debug.LogWarning($"[SettingsMenuController] TabId '{tabId}' not found in tabsContainer.");
                continue;
            }

            GameObject tabButtonInstance = Instantiate(tabButtonPrefab, tabsButtonsContainer, false);
            if (!tabButtonInstance.TryGetComponent<TabButton>(out var tabButtonComponent))
            {
                Debug.LogError("[SettingsMenuController] TabButtonPrefab missing TabButton component.");
                Destroy(tabButtonInstance);
                continue;
            }

            tabButtonComponent.Setup(tab.TabId, tab.DisplayName, this);
            createdTabButtons.Add(tabButtonComponent);
        }

        string firstTabId = contextTabs.tabIds.FirstOrDefault(id => tabsById.ContainsKey(id));
        if (!string.IsNullOrEmpty(firstTabId))
        {
            SelectTab(firstTabId);
        }
    }

    /// <summary>
    /// Called when a tab is clicked. Shows the tab with the specified tabId.
    /// </summary>
    public void SelectTab(string tabId)
    {
        if (string.IsNullOrEmpty(tabId))
        {
            return;
        }

        if (!tabsById.TryGetValue(tabId, out var tab))
        {
            Debug.LogWarning($"[SettingsMenuController] SelectTab: tabId '{tabId}' not found.");
            return;
        }

        if (activeTab == tab)
        {
            return;
        }

        // Unselect old
        if (activeTab != null)
        {
            activeTab.OnTabUnselected();
            activeTab.gameObject.SetActive(false);
        }

        // Activate new
        activeTab = tab;
        activeTab.gameObject.SetActive(true);
        activeTab.OnTabSelected();

        foreach (TabButton tabButton in createdTabButtons)
        {
            bool isSelected = tabButton.TabId == tabId;
            tabButton.SetSelected(isSelected);
        }

        Debug.Log($"[SettingsMenuController] Selected tab '{tabId}'");
    }
}
