using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class CheckpointPath : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LevelObjectRegistry levelObjectRegistry;
    [SerializeField, Min(1)] private int pointsPerSegment = 20;

    private LineRenderer lineRenderer;

    private readonly List<Checkpoint> allCheckpoints = new();
    private List<Checkpoint> orderedValidCheckpoints = new();

    private bool scenarioActive;
    private bool rebuilding;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (levelObjectRegistry == null)
        {
            Debug.LogError("[CheckpointPath] LevelObjectRegistry is not assigned.");
        }
    }

    private void OnDisable()
    {
        Deactivate();
    }

    public void SetScenarioActive(bool active)
    {
        if (scenarioActive == active)
        {
            return;
        }

        scenarioActive = active;

        if (scenarioActive)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    private void Activate()
    {
        if (levelObjectRegistry == null)
        {
            ClearPath();
            return;
        }

        levelObjectRegistry.LevelObjectLifecycleChanged += OnLifecycleChanged;
        RebuildPath();
    }

    private void Deactivate()
    {
        if (levelObjectRegistry != null)
        {
            levelObjectRegistry.LevelObjectLifecycleChanged -= OnLifecycleChanged;
        }

        UnsubscribeFromCheckpoints();

        allCheckpoints.Clear();
        orderedValidCheckpoints.Clear();

        ClearPath();
    }

    private void OnLifecycleChanged(LevelObject levelObject)
    {
        if (levelObject is Checkpoint)
        {
            RebuildPath();
        }
    }

    private void OnCheckpointTransformChanged(LevelObject _)
    {
        if (!scenarioActive)
        {
            return;
        }

        // The validity of the sequence does not change depending on the position,
        // so we simply update the geometry if the path is already valid.
        UpdateLinePositions();
    }

    private void OnCheckpointPropertyChanged(LevelObject _, PropertyKey changedKey)
    {
        if (!scenarioActive)
        {
            return;
        }

        if (changedKey == LevelPropertyKeys.Index)
        {
            RebuildPath();
        }
    }

    public void RebuildPath()
    {
        if (!scenarioActive || rebuilding)
        {
            return;
        }

        if (levelObjectRegistry == null)
        {
            ClearPath();
            return;
        }

        rebuilding = true;

        try
        {
            UnsubscribeFromCheckpoints();

            allCheckpoints.Clear();
            allCheckpoints.AddRange(levelObjectRegistry.EnumerateAlive<Checkpoint>());

            // Subscribe to all checkpoints, even if the sequence is not yet valid.
            // Then, after the indexes are corrected, the path will automatically appear.
            SubscribeToCheckpoints();

            if (!CheckpointSequenceService.TryGetValidSequence(allCheckpoints, out var ordered))
            {
                orderedValidCheckpoints.Clear();
                ClearPath();
                return;
            }

            orderedValidCheckpoints = ordered;
            UpdateLinePositions();
        }
        finally
        {
            rebuilding = false;
        }
    }

    private void UpdateLinePositions()
    {
        if (orderedValidCheckpoints.Count < 2)
        {
            ClearPath();
            return;
        }

        List<Vector3> points = new((orderedValidCheckpoints.Count - 1) * pointsPerSegment + 1);

        for (int i = 0; i < orderedValidCheckpoints.Count - 1; i++)
        {
            Transform from = orderedValidCheckpoints[i].transform;
            Transform to = orderedValidCheckpoints[i + 1].transform;

            Vector3 p0 = from.position;
            Vector3 p1 = to.position;

            float segmentLength = Vector3.Distance(p0, p1);
            float tangentStrength = segmentLength * 0.33f;

            Vector3 t0 = p0 + from.forward * tangentStrength;
            Vector3 t1 = p1 - to.forward * tangentStrength;

            for (int j = 0; j <= pointsPerSegment; j++)
            {
                if (i > 0 && j == 0)
                {
                    continue;
                }

                float t = (float)j / pointsPerSegment;
                points.Add(CubicBezier(p0, t0, t1, p1, t));
            }
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    private void ClearPath()
    {
        lineRenderer.positionCount = 0;
    }

    private void SubscribeToCheckpoints()
    {
        foreach (Checkpoint checkpoint in allCheckpoints)
        {
            if (checkpoint == null)
            {
                continue;
            }

            checkpoint.TransformChanged += OnCheckpointTransformChanged;
            checkpoint.PropertyChanged += OnCheckpointPropertyChanged;
        }
    }

    private void UnsubscribeFromCheckpoints()
    {
        foreach (Checkpoint checkpoint in allCheckpoints)
        {
            if (checkpoint == null)
            {
                continue;
            }

            checkpoint.TransformChanged -= OnCheckpointTransformChanged;
            checkpoint.PropertyChanged -= OnCheckpointPropertyChanged;
        }
    }

    private static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;

        return uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
    }
}
