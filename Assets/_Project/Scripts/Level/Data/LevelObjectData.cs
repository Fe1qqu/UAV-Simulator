using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelObjectData
{
    public string objectId;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public List<LevelObjectProperty> properties = new();
}
