using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Scenario Runtime/Kamikaze")]
public class KamikazeScenarioRuntime : ScenarioRuntimeBase
{
    [SerializeField] private float minImpactForce = 5f;

    private ExplosionZone explosionZone;
    private bool droneInsideZone;

    private ScenarioResult pendingResult;

    protected override void StartScenarioInternal()
    {
        explosionZone = levelObjectRegistry.FindFirstAlive<ExplosionZone>();
        if (explosionZone == null)
        {
            Debug.LogError("[KamikazeScenarioRuntime] ExplosionZone not found.");
            return;
        }

        droneController.CollisionHappened += OnDroneCollisionHappened;

        explosionZone.ObjectEntered += OnExplosionZoneObjectEntered;
        explosionZone.ObjectExited += OnExplosionZoneObjectExited;

        if (droneController.TryGetComponent<DroneDeathHandler>(out var droneDeathHandler))
        {
            droneDeathHandler.ExplosionSpawned += OnExplosionSpawned;
        }
    }

    protected override void ResetScenarioInternal()
    {
        droneInsideZone = false;
        pendingResult = ScenarioResult.None;

        // Recovering a drone if it was hidden
        if (droneController.TryGetComponent<DroneDeathHandler>(out var droneDeathHandler))
        {
            droneDeathHandler.Restore();
        }
    }

    protected override void DisposeScenarioInternal()
    {
        if (explosionZone != null)
        {
            explosionZone.ObjectEntered -= OnExplosionZoneObjectEntered;
            explosionZone.ObjectExited -= OnExplosionZoneObjectExited;
        }

        if (droneController != null)
        {
            droneController.CollisionHappened -= OnDroneCollisionHappened;

            if (droneController.TryGetComponent<DroneDeathHandler>(out var deathHandler))
            {
                deathHandler.ExplosionSpawned -= OnExplosionSpawned;
            }
        }
    }

    private void OnExplosionZoneObjectEntered(Collider collider)
    {
        var drone = collider.GetComponentInParent<IDroneActor>();
        if (drone is MonoBehaviour monoBehaviour && monoBehaviour.gameObject != droneController.gameObject)
        {
            return;
        }

        droneInsideZone = true;
    }

    private void OnExplosionZoneObjectExited(Collider collider)
    {
        var drone = collider.GetComponentInParent<IDroneActor>();
        if (drone is MonoBehaviour monoBehaviour && monoBehaviour.gameObject != droneController.gameObject)
        {
            return;
        }

        droneInsideZone = false;
    }

    private void OnDroneCollisionHappened(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;

        Debug.Log($"[KamikazeScenarioRuntime] Impact: {impact}.");

        if (impact < minImpactForce)
        {
            return;
        }

        pendingResult = droneInsideZone ? ScenarioResult.Success : ScenarioResult.Fail;

        ConcludeGameplay();

        droneController.Explode();
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
