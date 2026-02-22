using UnityEngine;

public class EditorLevelDataBuilder : MonoBehaviour
{
    [SerializeField] private Transform levelRoot;
    [SerializeField] private LevelObjectRegistry levelObjectRegistry;

    private void Awake()
    {
        if (levelRoot == null)
        {
            Debug.LogError("[EditorLevelDataBuilder] LevelRoot is not assigned.");
        }

        if (levelObjectRegistry == null)
        {
            Debug.LogError("[EditorLevelDataBuilder] LevelObjectRegistry is not assigned.");
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
