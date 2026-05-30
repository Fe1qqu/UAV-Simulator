using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;
using System;
using System.Linq;

public class ModalDialogController : MonoBehaviour, IBackHandler
{
    [Header("UI")]
    [SerializeField] private RectTransform windowRoot; // Window with VerticalLayoutGroup and ContentSizeFitter
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private ModalButton buttonPrefab;

    private Action<ModalResult> onResult;

    private ModalButton backActionButton;

    private const string Bullet = "• ";

    public bool IsShown => gameObject.activeSelf;

    private void Awake()
    {
        if (windowRoot == null)
        {
            Debug.LogError("[ModalDialogController] WindowRoot not assigned.");
        }

        if (messageText == null)
        {
            Debug.LogError("[ModalDialogController] MessageText not assigned.");
        }

        if (buttonsContainer == null)
        {
            Debug.LogError("[ModalDialogController] ButtonsContainer not assigned.");
        }

        if (buttonPrefab == null)
        {
            Debug.LogError("[ModalDialogController] ButtonPrefab not assigned.");
        }
    }

    public void Show(ModalDialogConfig config)
    {
        if (IsShown)
        {
            Debug.LogWarning("[ModalDialogController] Show called while dialog is already shown.");
            return;
        }

        onResult = config.OnResult;

        ClearButtons();
        CreateButtons(config);

        gameObject.SetActive(true);


        messageText.alignment = config.messageAlignment;

        var sb = new System.Text.StringBuilder();

        // 1. TITLE (no bullets)
        if (!string.IsNullOrEmpty(config.titleLocalizationKey))
        {
            var title = new LocalizedString("UI", config.titleLocalizationKey)
            {
                Arguments = config.titleArguments
            };

            sb.AppendLine(title.GetLocalizedString());
        }

        // 2. SINGLE MESSAGE (optional)
        if (!string.IsNullOrEmpty(config.messageLocalizationKey))
        {
            var message = new LocalizedString("UI", config.messageLocalizationKey)
            {
                Arguments = config.messageArguments
            };

            sb.AppendLine(message.GetLocalizedString());
        }

        // 3. BULLET LIST (validation/errors)
        if (config.messageLines != null && config.messageLines.Count > 0)
        {
            foreach (var line in config.messageLines)
            {
                sb.Append("• ");
                sb.AppendLine(line);
            }
        }

        messageText.text = sb.ToString().TrimEnd();

        BackDispatcher.RegisterHandler(this);

        LayoutRebuilder.ForceRebuildLayoutImmediate(windowRoot);
    }

    public void Hide()
    {
        //if (!IsShown)
        //{
        //    Debug.LogWarning("[ModalDialogController] Hide called while dialog is already hidden.");
        //    return;
        //}

        gameObject.SetActive(false);

        BackDispatcher.UnregisterHandler(this);
    }

    private void CreateButtons(ModalDialogConfig modalDialogConfig)
    {
        backActionButton = null;

        foreach (ModalButtonConfig modalButtonConfig in modalDialogConfig.Buttons)
        {
            ModalButton button = Instantiate(buttonPrefab, buttonsContainer);
            button.gameObject.SetActive(true);
            button.Initialize(modalButtonConfig.localizationTableEntryKey, modalButtonConfig.Result, OnButtonResult);

            if (modalButtonConfig.IsBackAction)
            {
                if (backActionButton != null)
                {
                    Debug.LogWarning("[ModalDialogController] Multiple BackAction buttons specified. Last one will be used.");
                }

                backActionButton = button;
            }
        }

        if (backActionButton == null)
        {
            Debug.LogWarning("[ModalDialogController] No BackAction button specified.");
        }
    }

    private void ClearButtons()
    {
        for (int i = buttonsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonsContainer.GetChild(i).gameObject);
        }
    }

    private void OnButtonResult(ModalResult result)
    {
        Hide();
        onResult?.Invoke(result);
    }

    public bool OnBack()
    {
        if (backActionButton == null)
        {
            return false;
        }

        backActionButton.Invoke();
        return true;
    }
}
