using UnityEngine;

public class LevelObject : MonoBehaviour
{
    [SerializeField, HideInInspector] private string objectId;

    public string ObjectId => objectId;

    public void SetObjectId(string id)
    {
        objectId = id;
    }

    public virtual LevelObjectData ToData()
    {
        return new LevelObjectData
        {
            objectId = objectId,
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.localScale
        };
    }

    public virtual void FromData(LevelObjectData data)
    {
        transform.position = data.position;
        transform.rotation = data.rotation;
        transform.localScale = data.scale;
    }
}
