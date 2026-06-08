using UnityEngine;

public class PreviewObject : MonoBehaviour
{
    private Renderer[] renderers;
    private Collider[] colliders;

    private Material[][] originalMaterials;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);

        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterials;
        }
    }

    public void EnablePreviewMode(PreviewMaterialMode materialMode, Material defaultPreviewMaterial, Material overrideMaterial)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            switch (materialMode)
            {
                case PreviewMaterialMode.UseDefault:
                { 
                    Material[] materials = new Material[renderer.sharedMaterials.Length];

                    for (int j = 0; j < materials.Length; j++)
                    {
                        materials[j] = defaultPreviewMaterial;
                    }

                    renderer.sharedMaterials = materials;
                    break;
                }
                case PreviewMaterialMode.Override:
                {
                    if (overrideMaterial != null)
                    {
                        Material[] materials = new Material[renderer.sharedMaterials.Length];

                        for (int j = 0; j < materials.Length; j++)
                        {
                            materials[j] = overrideMaterial;
                        }

                        renderer.sharedMaterials = materials;
                    }
                    break;
                }
                case PreviewMaterialMode.None:
                    break;
            }

            renderer.enabled = false;
        }

        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    public void DisablePreviewMode()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterials = originalMaterials[i];
            renderers[i].enabled = true;
        }

        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }
    }

    public void SetVisible(bool visible)
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }
}
