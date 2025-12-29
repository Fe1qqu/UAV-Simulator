using UnityEngine;

public class EditorLevelDataBuilder : MonoBehaviour
{
    [SerializeField] private Transform levelRoot;

    private void Awake()
    {
        if (levelRoot == null)
        {
            Debug.LogError("[EditorLevelDataBuilder] LevelRoot not assigned.");
        }
    }

    public LevelData CollectLevelData()
    {
        EditorSession editorSession = GameSettings.Instance.CurrentEditorSession;

        LevelData data = new LevelData
        {
            levelName = editorSession.LevelName,
            locationId = editorSession.SelectedLocationId
        };

        foreach (Transform child in levelRoot)
        {
            if (!child.TryGetComponent<LevelObject>(out var levelObject))
            {
                continue;
            }

            data.objects.Add(levelObject.ToData());
        }

        return data;
    }
}
