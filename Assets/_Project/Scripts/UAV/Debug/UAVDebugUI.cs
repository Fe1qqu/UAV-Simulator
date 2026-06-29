using UnityEngine;

public class UAVDebugUI : MonoBehaviour
{
    private UAVControllerBase uavController;
    private QuadcopterController quadcopterController;

    private bool visible = true;
    private Rect windowRectangle = new(20, 20, 250, 270);

    // Стили инициализируются один раз в первом OnGUI
    private GUIStyle _badgeStyle;
    private bool _stylesInitialized;

    private void Awake()
    {
        uavController = GetComponent<UAVControllerBase>();
        if (uavController == null)
        {
            Debug.LogError($"[UAVDebugUI] No UAVControllerBase found on {gameObject.name}.");
            enabled = false;
            return;
        }

        // QuadcopterController нужен только для отображения FlightMode
        quadcopterController = GetComponent<QuadcopterController>();
    }

    private void InitStyles()
    {
        if (_stylesInitialized) return;

        _badgeStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize  = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
        };
        _badgeStyle.normal.textColor = Color.white;

        _stylesInitialized = true;
    }

    private void OnGUI()
    {
        if (!visible) return;

        InitStyles();

        windowRectangle = GUI.Window(0, windowRectangle, DrawWindow, "UAV Debug Info");

        if (quadcopterController != null)
            DrawFlightModeBadge();
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginVertical();

        GUILayout.Label("<b>Input</b>");
        GUILayout.Label($"Throttle: {uavController.ThrottleInput:F2}");
        GUILayout.Label($"Yaw:      {uavController.YawInput:F2}");
        GUILayout.Label($"Pitch:    {uavController.PitchInput:F2}");
        GUILayout.Label($"Roll:     {uavController.RollInput:F2}");
        GUILayout.Space(5);

        var rotors = uavController.DebugRotorRPMs;
        if (rotors != null)
        {
            GUILayout.Label("<b>Rotors</b>");
            foreach (var kv in rotors)
                GUILayout.Label($"{kv.Key}: {kv.Value:F0} RPM");
        }

        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    // ── Бейдж режима полёта — правый нижний угол ──────────────────────────
    private void DrawFlightModeBadge()
    {
        const float badgeW = 120f;
        const float badgeH = 36f;
        const float margin = 16f;

        float x = Screen.width  - badgeW - margin;
        float y = Screen.height - badgeH - margin;

        FlightMode mode = quadcopterController.CurrentFlightMode;

        string label    = ModeLabel(mode);
        Color  bgColor  = ModeColor(mode);

        // Фон
        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = bgColor;
        GUI.Box(new Rect(x, y, badgeW, badgeH), GUIContent.none, _badgeStyle);
        GUI.backgroundColor = prev;

        // Текст поверх
        GUI.Label(new Rect(x, y, badgeW, badgeH), label, _badgeStyle);
    }

    /// <summary>Короткая метка режима для HUD.</summary>
    private static string ModeLabel(FlightMode mode) => mode switch
    {
        FlightMode.AngleLimited   => "ANGLE",
        FlightMode.AngleUnlimited => "ACRO",
        FlightMode.AltHold        => "ALT HOLD",
        _                         => mode.ToString().ToUpper()
    };

    /// <summary>Цвет фона бейджа по режиму.</summary>
    private static Color ModeColor(FlightMode mode) => mode switch
    {
        FlightMode.AngleLimited   => new Color(0.15f, 0.45f, 0.85f, 0.85f),  // синий
        FlightMode.AngleUnlimited => new Color(0.80f, 0.30f, 0.10f, 0.85f),  // красный/оранжевый
        FlightMode.AltHold        => new Color(0.10f, 0.60f, 0.25f, 0.85f),  // зелёный
        _                         => new Color(0.3f,  0.3f,  0.3f,  0.85f)
    };

    public void ToggleVisibility()
    {
        visible = !visible;
    }
}
