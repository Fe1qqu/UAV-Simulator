    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(CanvasGroup))]
    public class TabButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        //[SerializeField] private GameObject selectedIndicator;

        // CanvasGroup used to visually highlight selected state
        private CanvasGroup canvasGroup;

        private Button button;
        private SettingsMenuController settingsMenuController;

        public string TabId { get; private set; }

        private void Awake()
        {
            button = GetComponent<Button>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (label == null)
            {
                Debug.LogError("[TabButton] Label is not assigned.");
            }
        }

        public void Setup(string tabId, string title, SettingsMenuController settingsMenuController)
        {
            TabId = tabId;
            this.settingsMenuController = settingsMenuController;

            if (label != null)
            {
                label.text = title ?? tabId;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);

            SetSelected(false);
        }

        private void OnClicked()
        {
            if (settingsMenuController == null)
            {
                Debug.LogError("[TabButton] SettingsMenuController is null.");
            }

            settingsMenuController.SelectTab(TabId);
        }

        public void SetSelected(bool selected)
        {
            canvasGroup.alpha = selected ? 0.6f : 1.0f;
        }
    }
