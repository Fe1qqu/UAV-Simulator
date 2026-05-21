using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(LineRenderer))]
public class CheckpointPath : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LevelObjectRegistry levelObjectRegistry;
    [SerializeField] private int pointsPerSegment = 20;

    private LineRenderer lineRenderer;

    private readonly List<Checkpoint> allCheckpoints = new();
    private List<Checkpoint> checkpoints = new();

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

    private void OnLifecycleChanged(LevelObject levelObject)
    {
        if (levelObject is Checkpoint)
        {
            RebuildPath();
        }
    }

    private void UnsubscribeFromCheckpoints()
    {
        foreach (Checkpoint checkpoint in allCheckpoints)
        {
            if (checkpoint != null)
            {
                checkpoint.TransformChanged -= OnCheckpointTransformChanged;
                checkpoint.PropertyChanged -= OnCheckpointPropertyChanged;
            }
        }
    }

    private void SubscribeToCheckpoints()
    {
        foreach (Checkpoint checkpoint in allCheckpoints)
        {
            checkpoint.TransformChanged += OnCheckpointTransformChanged;
            checkpoint.PropertyChanged += OnCheckpointPropertyChanged;
        }
    }

    private void OnCheckpointTransformChanged(LevelObject _)
    {
        if (!scenarioActive)
        {
            return;
        }

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
        if (levelObjectRegistry != null)
        {
            levelObjectRegistry.LevelObjectLifecycleChanged += OnLifecycleChanged;
        }

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
        checkpoints.Clear();

        lineRenderer.positionCount = 0;
    }

    public void RebuildPath()
    {
        if (!scenarioActive)
        {
            return;
        }

        if (rebuilding)
        {
            return;
        }

        rebuilding = true;

        UnsubscribeFromCheckpoints();

        allCheckpoints.Clear();

        allCheckpoints.AddRange(
            levelObjectRegistry.EnumerateAlive<Checkpoint>()
        );

        checkpoints = allCheckpoints
            .GroupBy(checkpoint => checkpoint.Get(LevelPropertyKeys.Index, 0))
            .Select(group => group.First())
            .OrderBy(checkpoint => checkpoint.Get(LevelPropertyKeys.Index, 0))
            .ToList();

        SubscribeToCheckpoints();

        UpdateLinePositions();

        rebuilding = false;
    }

    private void UpdateLinePositions()
    {
        if (checkpoints.Count < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        List<Vector3> points = new();

        for (int i = 0; i < checkpoints.Count - 1; i++)
        {
            Vector3 p0 = checkpoints[i].transform.position;
            Vector3 p1 = checkpoints[i + 1].transform.position;

            float segmentLength = Vector3.Distance(p0, p1);
            float tangentStrength = segmentLength * 0.33f;

            Vector3 t0 = p0 + checkpoints[i].transform.forward * tangentStrength;
            Vector3 t1 = p1 - checkpoints[i + 1].transform.forward * tangentStrength;

            for (int j = 0; j <= pointsPerSegment; j++)
            {
                // Skip the first point of the segment if it is not the first segment in order to avoid duplicating points at the joints
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
