using UnityEngine;

public class LevelObject : MonoBehaviour
{
    [SerializeField, HideInInspector] private string objectId;

    public string ObjectId => objectId;

    public void Initialize(string objectId)
    {
        if (string.IsNullOrEmpty(objectId))
        {
            Debug.LogError($"[LevelObject] Initialize called with empty ObjectId on {name}.");
            return;
        }

        this.objectId = objectId;
    }

    public LevelObjectData ToData()
    {
        if (string.IsNullOrEmpty(objectId))
        {
            Debug.LogError($"[LevelObject] ObjectId is empty on {name}.");
        }

        return new LevelObjectData
        {
            objectId = objectId,
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.localScale
        };
    }

    public void FromData(LevelObjectData data)
    {
        if (!string.IsNullOrEmpty(data.objectId))
        {
            objectId = data.objectId;
        }
        else
        {
            Debug.LogError($"[LevelObject] ObjectId is empty on {name}.");
        }

        objectId = data.objectId;
        transform.position = data.position;
        transform.rotation = data.rotation;
        transform.localScale = data.scale;
    }
}
