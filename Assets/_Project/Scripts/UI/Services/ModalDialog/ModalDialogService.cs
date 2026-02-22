using UnityEngine;

public class ModalDialogService : MonoBehaviour
{
    [SerializeField] private ModalDialogController modalDialogController;

    public bool IsModalShown => modalDialogController != null && modalDialogController.IsShown;

    private void Awake()
    {
        if (modalDialogController == null)
        {
            Debug.LogError("[ModalDialogService] ModalDialogController is not assigned.");
            return;
        }

        modalDialogController.Hide();
    }

    public void Show(ModalDialogConfig config)
    {
        if (modalDialogController == null)
        {
            Debug.LogError("[ModalDialogService] ModalDialogController is missing.");
            return;
        }

        modalDialogController.Show(config);
    }
}
