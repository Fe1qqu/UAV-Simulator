using UnityEngine;
using UnityEngine.EventSystems;

public class UINavigatorContext : MonoBehaviour
{
    public static UINavigatorContext Instance { get; private set; }

    private GameObject lastSelected;

    private void Awake()
    {
        Instance = this;
    }

    public void ResetSelection()
    {
        lastSelected = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void RestoreSelection()
    {
        if (lastSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }
    }

    public void SetSelected(GameObject go)
    {
        lastSelected = go;
        EventSystem.current.SetSelectedGameObject(go);
    }
}
