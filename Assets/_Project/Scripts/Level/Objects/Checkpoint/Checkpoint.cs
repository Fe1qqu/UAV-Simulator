using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class Checkpoint : LevelObject, ITriggerReceiver
{
    public event Action<Checkpoint> OnEntered;

    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private Material unpassedMaterial;
    [SerializeField] private Material passedMaterial;
    [SerializeField] private TMP_Text indexNumberText;

    [Header("Direction")]
    [SerializeField] private float maxApproachAngle = 60f;

    private bool isPassed;

    private readonly HashSet<Transform> uavsInTrigger = new();

    private void Awake()
    {
        if (meshRenderer == null)
        {
            Debug.LogError("[Checkpoint] MeshRenderer is not assigned.");
        }

        if (unpassedMaterial == null)
        {
            Debug.LogError("[Checkpoint] UnpassedMaterial is not assigned.");
        }

        if (passedMaterial == null)
        {
            Debug.LogError("[Checkpoint] PassedMaterial is not assigned.");
        }

        if (indexNumberText == null)
        {
            Debug.LogError("[Checkpoint] IndexNumberText is not assigned.");
        }

        ResetState();
    }

    private void OnEnable()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void OnDisable()
    {
        PropertyChanged -= OnPropertyChanged;
    }

    private void OnPropertyChanged(LevelObject levelObject, PropertyKey changedKey)
    {
        if (changedKey == LevelPropertyKeys.Index)
        {
            UpdateIndexVisual();
        }
    }

    private void UpdateIndexVisual()
    {
        int index = Get(LevelPropertyKeys.Index, -1);
        indexNumberText.text = index >= 0 ? index.ToString() : "?";
    }

    public void MarkPassed()
    {
        if (isPassed)
        {
            return;
        }

        isPassed = true;
        meshRenderer.material = passedMaterial;
    }

    public void ResetState()
    {
        isPassed = false;
        meshRenderer.sharedMaterial = unpassedMaterial;
    }

    public void OnTriggerEntered(Collider collider)
    {
        if (isPassed)
        {
            return;
        }

        var uav = collider.GetComponentInParent<IUAVActor>();
        if (uav == null)
        {
            Debug.LogWarning(
                $"[Checkpoint] Non-uav object entered trigger. Checkpoint: {name}, " +
                $"Object: {collider.name}, Layer: {LayerMask.LayerToName(collider.gameObject.layer)}."
            );

            return;
        }

        Transform uavTransform = ((MonoBehaviour)uav).transform;
        
        if (uavsInTrigger.Contains(uavTransform))
        {
            return;
        }

        uavsInTrigger.Add(uavTransform);

        if (!IsApproachValid(uavTransform))
        {
            Debug.Log($"[Checkpoint] Wrong direction approach on checkpoint {name}.");
            return;
        }

        OnEntered?.Invoke(this);
    }

    public void OnTriggerExited(Collider collider)
    {
        var uav = collider.GetComponentInParent<IUAVActor>();
        if (uav == null)
        {
            return;
        }

        Transform uavTransform = ((MonoBehaviour)uav).transform;

        uavsInTrigger.Remove(uavTransform);
    }

    private bool IsApproachValid(Transform uavTransform)
    {
        float angle = Vector3.Angle(uavTransform.forward, transform.forward);
        return angle <= maxApproachAngle;
    }
}
