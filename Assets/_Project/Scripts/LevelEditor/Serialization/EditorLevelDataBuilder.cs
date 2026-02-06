using UnityEngine;

public class EditorLevelDataBuilder : MonoBehaviour
{
    [SerializeField] private Transform levelRoot;
    [SerializeField] private LevelRuntimeRegistry levelRuntimeRegistry;

    private void Awake()
    {
        if (levelRoot == null)
        {
            Debug.LogError("[EditorLevelDataBuilder] LevelRoot is not assigned.");
        }

        if (levelRuntimeRegistry == null)
        {
            Debug.LogError("[EditorLevelDataBuilder] LevelRuntimeRegistry is not assigned.");
        }
    }

    public LevelData CollectLevelData()
    {
        EditorSession editorSession = GameSettings.Instance.CurrentEditorSession;

        LevelData data = new()
        {
            levelName = editorSession.LevelName,
            locationId = editorSession.SelectedLocationId,
            scenarioId = editorSession.SelectedScenarioId
        };

        foreach (LevelObject levelObject in levelRuntimeRegistry.LevelObjects)
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
