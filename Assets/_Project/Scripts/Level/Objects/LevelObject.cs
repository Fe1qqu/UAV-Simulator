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
    [SerializeField, HideInInspector] private PlaceableObjectData sourceData;
    [SerializeField, HideInInspector] private List<LevelObjectProperty> properties = new();

    public PlaceableObjectData SourceData => sourceData;
    public IReadOnlyList<LevelObjectProperty> Properties => properties;

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

    public void Initialize(PlaceableObjectData sourceData)
    {
        if (sourceData == null)
        {
            Debug.LogError("[LevelObject] Initialize called with null PlaceableObjectData.");
            return;
        }

        this.sourceData = sourceData;
        LifecycleState = LevelObjectLifecycleState.Alive;
        gameObject.SetActive(true);

        foreach (ObjectPropertyDefinition propertyDefinition in sourceData.propertyDefinitions)
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
        if (string.IsNullOrEmpty(sourceData.objectId))
        {
            Debug.LogError($"[LevelObject] ObjectId is empty during ToData on '{name}'.");
        }

        return new LevelObjectData
        {
            objectId = sourceData.objectId,
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.localScale,
            properties = new List<LevelObjectProperty>(properties)
        };
    }

    public void FromData(LevelObjectData data)
    {
        if (data == null)
        {
            Debug.LogError($"[LevelObject] FromData called with null data on '{name}'.");
            return;
        }

        if (string.IsNullOrEmpty(data.objectId))
        {
            Debug.LogError($"[LevelObject] ObjectId is empty in LevelObjectData on '{name}'.");
        }
        else
        {
            sourceData.objectId = data.objectId;
        }

        transform.SetPositionAndRotation(data.position, data.rotation);
        transform.localScale = data.scale;

        properties = new List<LevelObjectProperty>();

        foreach (ObjectPropertyDefinition propertyDefinition in sourceData.propertyDefinitions)
        {
            LevelObjectProperty saved = data.properties?.Find(property => property.key == propertyDefinition.key);

            properties.Add(new LevelObjectProperty
            {
                key = propertyDefinition.key,
                value = saved != null ? saved.value : propertyDefinition.defaultValue
            });
        }
    }
}
