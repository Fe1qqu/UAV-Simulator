using UnityEngine;

public static class TransformUiAdapter
{
    // UI: X, Y, Z (Z = up)
    // Unity: X, Y = up, Z

    public static Vector3 UiToUnityPosition(Vector3 ui)
    {
        return new Vector3(
            ui.x,
            ui.z, // UI Z → Unity Y
            ui.y  // UI Y → Unity Z
        );
    }

    public static Vector3 UnityToUiPosition(Vector3 unity)
    {
        return new Vector3(
            unity.x,
            unity.z, // Unity Z → UI Y
            unity.y  // Unity Y → UI Z (up)
        );
    }

    public static Vector3 UiToUnityRotation(Vector3 uiEuler)
    {
        return new Vector3(
            uiEuler.x,
            uiEuler.z,
            uiEuler.y
        );
    }

    public static Vector3 UnityToUiRotation(Vector3 unityEuler)
    {
        return new Vector3(
            unityEuler.x,
            unityEuler.z,
            unityEuler.y
        );
    }

    public static Vector3 UiToUnityScale(Vector3 ui)
    {
        return new Vector3(ui.x, ui.z, ui.y);
    }

    public static Vector3 UnityToUiScale(Vector3 unity)
    {
        return new Vector3(unity.x, unity.z, unity.y);
    }
}
