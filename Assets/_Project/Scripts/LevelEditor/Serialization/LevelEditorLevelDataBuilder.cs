using UnityEngine;

public class LevelEditorLevelDataBuilder : MonoBehaviour
{
    [SerializeField] private Transform levelRoot;
    [SerializeField] private LevelObjectRegistry levelObjectRegistry;

    private void Awake()
    {
        if (levelRoot == null)
        {
            Debug.LogError("[LevelEditorLevelDataBuilder] LevelRoot is not assigned.");
        }

        if (levelObjectRegistry == null)
        {
            Debug.LogError("[LevelEditorLevelDataBuilder] LevelObjectRegistry is not assigned.");
        }
    }

    public LevelData CollectLevelData()
    {
        LevelEditorSession levelEditorSession = GameSession.Instance.LevelEditor;

        LevelData data = new()
        {
            levelName = levelEditorSession.LevelName,
            locationId = levelEditorSession.LocationId,
            scenarioId = levelEditorSession.ScenarioId
        };

        foreach (LevelObject levelObject in levelObjectRegistry.LevelObjects)
        {
            // Ignore inactive objects (deleted / hidden)
            if (!levelObject.IsAlive)
            {
                continue;
            }

            data.objects.Add(levelObject.ToData());
        }

        return data;
    }
}
