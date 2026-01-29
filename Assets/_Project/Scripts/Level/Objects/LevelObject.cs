using UnityEngine;

public enum LevelObjectLifecycleState
{
    Alive,        // Object exists and active
    SoftDeleted,  // Temporarily hidden (undo/redo)
    HardDeleted   // Destroyed forever
}

public class LevelObject : MonoBehaviour
{
    [SerializeField, HideInInspector] private PlaceableObjectData sourceData;

    public PlaceableObjectData SourceData => sourceData;

    public LevelObjectLifecycleState LifecycleState { get; private set; }

    public bool IsAlive => LifecycleState == LevelObjectLifecycleState.Alive;

    private void OnDestroy()
    {
        if (LevelRuntimeRegistry.Instance != null)
        {
            // Hard delete (scene unload, cleanup, purge)
            LevelRuntimeRegistry.Instance.Unregister(this);
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

        if (LevelRuntimeRegistry.Instance != null)
        {
            LevelRuntimeRegistry.Instance.Register(this);
            LevelRuntimeRegistry.Instance.NotifyLifecycleChanged(this);
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

        if (LevelRuntimeRegistry.Instance != null)
        {
            LevelRuntimeRegistry.Instance.NotifyLifecycleChanged(this);
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

        if (LevelRuntimeRegistry.Instance != null)
        {
            LevelRuntimeRegistry.Instance.NotifyLifecycleChanged(this);
        }
    }

    public void HardDelete()
    {
        if (LifecycleState == LevelObjectLifecycleState.HardDeleted)
        {
            return;
        }

        LifecycleState = LevelObjectLifecycleState.HardDeleted;

        if (LevelRuntimeRegistry.Instance != null)
        {
            LevelRuntimeRegistry.Instance.NotifyLifecycleChanged(this);
        }

        Destroy(gameObject);
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
            scale = transform.localScale
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

        transform.position = data.position;
        transform.rotation = data.rotation;
        transform.localScale = data.scale;
    }
}
