using UnityEngine;
using UnityEngine.VFX;
using System;
using System.Collections;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private VisualEffect visualEffect;

    public event Action Finished;

    void Start()
    {
        if (visualEffect == null)
        {
            Debug.LogError("[ExplosionEffect] VisualEffect is not assigned.");
        }

        StartCoroutine(WaitForVFXToEnd());
    }

    IEnumerator WaitForVFXToEnd()
    {
        yield return new WaitForSeconds(0.1f);

        yield return new WaitUntil(() => !visualEffect.HasAnySystemAwake());

        Finished?.Invoke();

        yield return null;

        Destroy(gameObject);
    }
}
