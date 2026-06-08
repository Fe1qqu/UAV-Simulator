using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Scenario Runtime/Kamikaze")]
public class KamikazeScenarioRuntime : ScenarioRuntimeBase
{
    [SerializeField] private float minImpactForce = 5f;

    private ExplosionZone explosionZone;
    private bool uavInsideZone;

    private ScenarioResult pendingResult;

    protected override void StartScenarioInternal()
    {
        explosionZone = levelObjectRegistry.FindFirstAlive<ExplosionZone>();
        if (explosionZone == null)
        {
            Debug.LogError("[KamikazeScenarioRuntime] ExplosionZone not found.");
            return;
        }

        uavController.CollisionHappened += OnUAVCollisionHappened;

        explosionZone.ObjectEntered += OnExplosionZoneObjectEntered;
        explosionZone.ObjectExited += OnExplosionZoneObjectExited;

        if (uavController.TryGetComponent<UAVDeathHandler>(out var uavDeathHandler))
        {
            uavDeathHandler.ExplosionSpawned += OnExplosionSpawned;
        }
    }

    protected override void ResetScenarioInternal()
    {
        uavInsideZone = false;
        pendingResult = ScenarioResult.None;

        // Recovering a uav if it was hidden
        if (uavController.TryGetComponent<UAVDeathHandler>(out var uavDeathHandler))
        {
            uavDeathHandler.Restore();
        }
    }

    protected override void DisposeScenarioInternal()
    {
        if (explosionZone != null)
        {
            explosionZone.ObjectEntered -= OnExplosionZoneObjectEntered;
            explosionZone.ObjectExited -= OnExplosionZoneObjectExited;
        }

        if (uavController != null)
        {
            uavController.CollisionHappened -= OnUAVCollisionHappened;

            if (uavController.TryGetComponent<UAVDeathHandler>(out var deathHandler))
            {
                deathHandler.ExplosionSpawned -= OnExplosionSpawned;
            }
        }
    }

    private void OnExplosionZoneObjectEntered(Collider collider)
    {
        var uav = collider.GetComponentInParent<IUAVActor>();
        if (uav is MonoBehaviour monoBehaviour && monoBehaviour.gameObject != uavController.gameObject)
        {
            return;
        }

        uavInsideZone = true;
    }

    private void OnExplosionZoneObjectExited(Collider collider)
    {
        var uav = collider.GetComponentInParent<IUAVActor>();
        if (uav is MonoBehaviour monoBehaviour && monoBehaviour.gameObject != uavController.gameObject)
        {
            return;
        }

        uavInsideZone = false;
    }

    private void OnUAVCollisionHappened(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;

        Debug.Log($"[KamikazeScenarioRuntime] Impact: {impact}.");

        if (impact < minImpactForce)
        {
            return;
        }

        pendingResult = uavInsideZone ? ScenarioResult.Success : ScenarioResult.Fail;

        ConcludeGameplay();

        uavController.Explode();
    }

    private void OnExplosionSpawned(ExplosionEffect effect)
    {
        void handler()
        {
            effect.Finished -= handler;

            switch (pendingResult)
            {
                case ScenarioResult.Success:
                    CompleteScenario();
                    break;

                case ScenarioResult.Fail:
                    FailScenario();
                    break;
            }
        }

        effect.Finished += handler;
    }
}
