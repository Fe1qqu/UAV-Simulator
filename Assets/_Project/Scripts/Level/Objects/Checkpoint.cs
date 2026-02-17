using UnityEngine;
using TMPro;
using System;
using System.Linq;

public class Checkpoint : LevelObject, ITriggerReceiver
{
    public event Action<Checkpoint> OnEntered;

    [SerializeField] private Renderer meshRenderer;
    [SerializeField] private Material unpassedMaterial;
    [SerializeField] private Material passedMaterial;

    [SerializeField] private TMP_Text indexNumberText;

    private bool isPassed;

    private const string IndexKey = "index";

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

    private void OnPropertyChanged(LevelObject levelObject, string changedKey)
    {
        if (changedKey != IndexKey)
        {
            Debug.LogWarning($"[Checkpoint] Property key != 'index'. Checkpoint: {name}.");
            return;
        }

        UpdateIndexVisual();
    }

    private void UpdateIndexVisual()
    {
        string value = Properties.FirstOrDefault(property => property.key == IndexKey)?.value;

        if (string.IsNullOrEmpty(value))
        {
            indexNumberText.text = "?";
            return;
        }

        indexNumberText.text = value;
    }

    public void MarkPassed()
    {
        if (isPassed)
        {
            return;
        }

        isPassed = true;
        meshRenderer.sharedMaterial = passedMaterial;
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

        var drone = collider.GetComponentInParent<IDroneActor>();
        if (drone == null)
        {
            Debug.LogWarning(
                $"[Checkpoint] Non-drone object entered trigger. Checkpoint: {name}, " +
                $"Object: {collider.name}, Layer: {LayerMask.LayerToName(collider.gameObject.layer)}."
            );

            return;
        }

        OnEntered?.Invoke(this);
    }

    public void OnTriggerExited(Collider collider) { }
}
