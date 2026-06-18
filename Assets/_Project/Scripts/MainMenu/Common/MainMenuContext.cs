using UnityEngine;

public class MainMenuContext : MonoBehaviour
{
    [Header("Persistence")]
    [SerializeField] private LevelFileManager levelFileManager;
    public LevelFileManager LevelFileManager => levelFileManager;


    private ILevelCatalog levelCatalog;
    public ILevelCatalog LevelCatalog => levelCatalog;

    private void Awake()
    {
        if (levelFileManager == null)
        {
            Debug.LogError("[MainMenuContext] LevelFileManager is not assigned.");
            return;
        }

        levelCatalog = new FileSystemLevelCatalog(levelFileManager);
    }
}
