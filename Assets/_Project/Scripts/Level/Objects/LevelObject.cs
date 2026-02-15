using UnityEngine;
using RTG;
using System;
using System.Collections.Generic;

public enum LevelObjectLifecycleState
{
    Alive,        // Object exists and active
    SoftDeleted,  // Temporarily hidden (undo/redo)
    HardDeleted   // Destroyed forever
}

public class LevelObject : MonoBehaviour
{
    [SerializeField, HideInInspector] private PlaceableObjectDefinition sourcePlaceableObject;
    [SerializeField, HideInInspector] private List<LevelObjectProperty> properties = new();

    public PlaceableObjectDefinition SourcePlaceableObject => sourcePlaceableObject;

    public IReadOnlyList<LevelObjectProperty> Properties => properties;
    public bool HasProperties => Properties != null && Properties.Count > 0;

    public LevelObjectLifecycleState LifecycleState { get; private set; }
    public bool IsAlive => LifecycleState == LevelObjectLifecycleState.Alive;

    public event Action<LevelObject> TransformChanged;
    public event Action<LevelObject, string> PropertyChanged;

    private void OnDestroy()
    {
        if (LevelObjectRegistry.Instance != null)
        {
            // Hard delete (scene unload, cleanup, purge)
            LevelObjectRegistry.Instance.Unregister(this);
        }
    }

    public void Initialize(PlaceableObjectDefinition placeableObject)
    {
        if (placeableObject == null)
        {
            Debug.LogError("[LevelObject] Initialize called with null PlaceableObject.");
            return;
        }

        this.sourcePlaceableObject = placeableObject;
        LifecycleState = LevelObjectLifecycleState.Alive;
        gameObject.SetActive(true);

        foreach (ObjectPropertyDefinition propertyDefinition in placeableObject.propertyDefinitions)
        {
            properties.Add(new LevelObjectProperty
            {
                key = propertyDefinition.key,
                value = propertyDefinition.defaultValue
            });
        }

        if (LevelObjectRegistry.Instance != null)
        {
            LevelObjectRegistry.Instance.Register(this);
            LevelObjectRegistry.Instance.NotifyLifecycleChanged(this);
        }
    }

    public void SoftDelete()
    {
        if (LifecycleState != LevelObjectLifecycleState.Alive)
        {
            return;
        }

        LifecycleState = LevelObjectLifecycleState.SoftDeleted;
        gameObject.SetActive(false);

        if (LevelObjectRegistry.Instance != null)
        {
            LevelObjectRegistry.Instance.NotifyLifecycleChanged(this);
        }
    }

    public void Restore()
    {
        if (LifecycleState != LevelObjectLifecycleState.SoftDeleted)
        {
            return;
        }

        LifecycleState = LevelObjectLifecycleState.Alive;
        gameObject.SetActive(true);

        if (LevelObjectRegistry.Instance != null)
        {
            LevelObjectRegistry.Instance.NotifyLifecycleChanged(this);
        }
    }

    public void HardDelete()
    {
        if (LifecycleState == LevelObjectLifecycleState.HardDeleted)
        {
            return;
        }

        LifecycleState = LevelObjectLifecycleState.HardDeleted;

        if (LevelObjectRegistry.Instance != null)
        {
            LevelObjectRegistry.Instance.NotifyLifecycleChanged(this);
        }

        Destroy(gameObject);
    }

    public void SetPosition(Vector3 newPosition, bool createUndo = false)
    {
        if (!IsAlive)
        {
            return;
        }

        if (createUndo)
        {
            RTUndoRedo.Get.RecordAction(new TransformChangeAction(this, transform.position, newPosition, TransformType.Position));
        }

        transform.position = newPosition;
        NotifyTransformChanged();
    }

    public void SetRotation(Vector3 eulerAngles, bool createUndo = false)
    {
        if (!IsAlive)
        {
            return;
        }

        if (createUndo)
        {
            RTUndoRedo.Get.RecordAction(new TransformChangeAction(this, transform.rotation.eulerAngles, eulerAngles, TransformType.Rotation));
        }

        transform.rotation = Quaternion.Euler(eulerAngles);
        NotifyTransformChanged();
    }

    public void SetScale(Vector3 newScale, bool createUndo = false)
    {
        if (!IsAlive)
        {
            return;
        }

        if (createUndo)
        {
            RTUndoRedo.Get.RecordAction(new TransformChangeAction(this, transform.localScale, newScale, TransformType.Scale));
        }

        transform.localScale = newScale;
        NotifyTransformChanged();
    }

    public void NotifyTransformChanged()
    {
        TransformChanged?.Invoke(this);
    }

    public bool TrySetProperty(string key, string newValue)
    {
        LevelObjectProperty property = properties.Find(property => property.key == key);
        if (property == null)
        {
            Debug.LogWarning($"[LevelObject] Property '{key}' not found.");
            return false;
        }

        if (property.value == newValue)
        {
            return false;
        }

        property.value = newValue;
        PropertyChanged?.Invoke(this, key);
        return true;
    }

    public LevelObjectData ToData()
    {
        if (string.IsNullOrEmpty(sourcePlaceableObject.objectId))
        {
            Debug.LogError($"[LevelObject] PlaceableObject.objectId is empty during ToData on '{name}'.");
        }

        return new LevelObjectData
        {
            objectId = sourcePlaceableObject.objectId,
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.localScale,
            properties = new List<LevelObjectProperty>(properties)
        };
    }

    public void FromData(LevelObjectData objectData)
    {
        if (objectData == null)
        {
            Debug.LogError($"[LevelObject] FromData called with null LevelObjectData on '{name}'.");
            return;
        }

        if (string.IsNullOrEmpty(objectData.objectId))
        {
            Debug.LogError($"[LevelObject] ObjectId is empty in LevelObjectData on '{name}'.");
        }
        else
        {
            sourcePlaceableObject.objectId = objectData.objectId;
        }

        transform.SetPositionAndRotation(objectData.position, objectData.rotation);
        transform.localScale = objectData.scale;

        properties = new List<LevelObjectProperty>();

        foreach (ObjectPropertyDefinition propertyDefinition in sourcePlaceableObject.propertyDefinitions)
        {
            LevelObjectProperty savedObjectProperty = objectData.properties?.Find(property => property.key == propertyDefinition.key);

            properties.Add(new LevelObjectProperty
            {
                key = propertyDefinition.key,
                value = savedObjectProperty != null ? savedObjectProperty.value : propertyDefinition.defaultValue
            });
        }
    }
}
