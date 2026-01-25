using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ModalButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    private ModalResult result;
    private Action<ModalResult> callback;

    public ModalResult Result => result;

    private void Awake()
    {
        if (button == null)
        {
            Debug.LogError("[ModalButton] Button not assigned.");
        }

        if (label == null)
        {
            Debug.LogError("[ModalButton] Label not assigned.");
        }
    }

    public void Initialize(string text, ModalResult result, Action<ModalResult> callback)
    {
        this.result = result;
        this.callback = callback;

        label.text = text;

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
