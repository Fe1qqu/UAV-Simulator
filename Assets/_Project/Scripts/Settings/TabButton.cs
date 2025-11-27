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
        private SettingsMenuController owner;

        public string PageId { get; private set; }

        private void Awake()
        {
            button = GetComponent<Button>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (label == null)
            {
                Debug.LogError("[TabButton] Label is not assigned.");
            }
        }

        public void Setup(string pageId, string displayName, SettingsMenuController owner)
        {
            PageId = pageId;
            this.owner = owner;

            if (label != null)
            {
                label.text = displayName ?? pageId;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);

            SetSelected(false);
        }

        private void OnClicked()
        {
            if (owner == null)
            {
                Debug.LogError("[TabButton] Owner is null.");
            }
        
            owner.SelectPage(PageId);
        }

        public void SetSelected(bool selected)
        {
            canvasGroup.alpha = selected ? 0.6f : 1.0f;
        }
    }
