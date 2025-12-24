using UnityEngine;

public class RuntimeLevelLoader : MonoBehaviour
{
    [SerializeField] private LevelSaveManager loader;

    private void Start()
    {
        //loader.Load(GameSettings.Instance.SelectedLevelFile);
    }
}
