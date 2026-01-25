using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;
using System;

public class ModalButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private LocalizeStringEvent labelLocalizeStringEvent;

    private ModalResult result;
    private Action<ModalResult> callback;

    public ModalResult Result => result;

    private void Awake()
    {
        if (button == null)
        {
            Debug.LogError("[ModalButton] Button not assigned.");
        }

        if (labelLocalizeStringEvent == null)
        {
            Debug.LogError("[ModalButton] LabelLocalizeStringEvent not assigned.");
        }
    }

    public void Initialize(string localizationKey, ModalResult result, Action<ModalResult> callback)
    {
        this.result = result;
        this.callback = callback;

        labelLocalizeStringEvent.StringReference.TableEntryReference = localizationKey;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        callback?.Invoke(result);
    }

    public void Invoke()
    {
        OnClicked();
    }
}
