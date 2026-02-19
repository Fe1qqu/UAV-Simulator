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
    public event Action<LevelObject, PropertyKey> PropertyChanged;

    private Dictionary<string, LevelObjectProperty> _propertyCache;

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

        sourcePlaceableObject = placeableObject;
        

        LifecycleState = LevelObjectLifecycleState.Alive;
        gameObject.SetActive(true);

        properties.Clear();
        foreach (ObjectPropertyDefinition propertyDefinition in placeableObject.propertyDefinitions)
        {
            properties.Add(new LevelObjectProperty
            {
                key = propertyDefinition.key,
                value = propertyDefinition.defaultValue
            });
        }
        BuildPropertyCache();

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

    protected void RaiseAllPropertiesChanged()
    {
        if (_propertyCache == null)
        {
            BuildPropertyCache();
        }

        foreach (var kvp in _propertyCache)
        {
            if (PropertyKeyRegistry.TryGet(kvp.Key, out var key))
            {
                PropertyChanged?.Invoke(this, key);
            }
        }
    }

    public bool HasProperty(PropertyKey key)
    {
        if (_propertyCache == null)
        {
            BuildPropertyCache();
        }

        return _propertyCache.ContainsKey(key.Name);
    }

    private void BuildPropertyCache()
    {
        _propertyCache = new Dictionary<string, LevelObjectProperty>();

        foreach (LevelObjectProperty property in properties)
        {
            _propertyCache[property.key] = property;
        }
    }

    public bool TryGet<T>(PropertyKey<T> key, out T value)
    {
        value = default;

        if (_propertyCache == null)
        {
            BuildPropertyCache();
        }

        if (!_propertyCache.TryGetValue(key.Name, out var property))
        {
            return false;
        }

        string raw = property.value;

        try
        {
            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(raw, System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out int i))
                {
                    value = (T)(object)i;
                    return true;
                }
            }
            else if (typeof(T) == typeof(float))
            {
                if (float.TryParse(raw, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float f))
                {
                    value = (T)(object)f;
                    return true;
                }
            }
            else if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(raw, out bool b))
                {
                    value = (T)(object)b;
                    return true;
                }
            }
            else if (typeof(T) == typeof(string))
            {
                value = (T)(object)raw;
                return true;
            }
            else if (typeof(T).IsEnum)
            {
                if (Enum.TryParse(typeof(T), raw, out object enumValue))
                {
                    value = (T)enumValue;
                    return true;
                }
            }
        }
        catch { }

        return false;
    }

    public T Get<T>(PropertyKey<T> key, T defaultValue = default)
    {
        return TryGet(key, out T value) ? value : defaultValue;
    }

    public string Get(PropertyKey key)
    {
        if (_propertyCache == null)
        {
            BuildPropertyCache();
        }

        if (!_propertyCache.TryGetValue(key.Name, out var property))
        {
            return null;
        }

        return property.value;
    }

    public bool Set<T>(PropertyKey<T> key, T newValue)
    {
        if (_propertyCache == null)
        {
            BuildPropertyCache();
        }

        if (!_propertyCache.TryGetValue(key.Name, out var property))
        {
            return false;
        }

        string stringValue = newValue is IFormattable f
                ? f.ToString(null, System.Globalization.CultureInfo.InvariantCulture)
                : newValue.ToString();

        if (property.value == stringValue)
        {
            return false;
        }

        property.value = stringValue;

        PropertyChanged?.Invoke(this, key);
        return true;
    }

    public bool Set(PropertyKey key, string rawValue)
    {
        if (_propertyCache == null)
        {
            BuildPropertyCache();
        }

        if (!_propertyCache.TryGetValue(key.Name, out var property))
        {
            return false;
        }

        if (property.value == rawValue)
        {
            return false;
        }

        property.value = rawValue;

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

        BuildPropertyCache();

        RaiseAllPropertiesChanged();
    }
}
