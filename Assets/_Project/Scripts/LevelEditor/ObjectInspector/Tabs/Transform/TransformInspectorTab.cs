using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Globalization;
using System.Collections.Generic;

public class TransformInspectorTab : MonoBehaviour
{
    [Header("Position")]
    [SerializeField] private TMP_InputField positionX;
    [SerializeField] private TMP_InputField positionY;
    [SerializeField] private TMP_InputField positionZ;

    [Header("Rotation")]
    [SerializeField] private TMP_InputField rotationX;
    [SerializeField] private TMP_InputField rotationY;
    [SerializeField] private TMP_InputField rotationZ;

    [Header("Scale")]
    [SerializeField] private TMP_InputField scaleX;
    [SerializeField] private TMP_InputField scaleY;
    [SerializeField] private TMP_InputField scaleZ;

    private LevelObject boundObject;
    private bool suppressCallbacks;
    private bool isDirty;
    private bool isActive;

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly Dictionary<TMP_InputField, UnityAction<string>> valueChangedHandlers = new();
    private readonly Dictionary<TMP_InputField, UnityAction<string>> endEditHandlers = new();

    private void Awake()
    {
        if (positionX == null || positionY == null || positionZ == null)
        {
            Debug.LogError("[TransformInspectorTab] Some of position input fields are not assigned.");
        }

        if (rotationX == null || rotationY == null || rotationZ == null)
        {
            Debug.LogError("[TransformInspectorTab] Some of rotation input fields are not assigned.");
        }

        if (scaleX == null || scaleY == null || scaleZ == null)
        {
            Debug.LogError("[TransformInspectorTab] Some of scale input fields are not assigned.");
        }

        RegisterCallbacks();
    }

    private void OnDestroy()
    {
        UnregisterCallbacks();
    }

    public void Bind(LevelObject levelObject)
    {
        if (boundObject != null)
        {
            boundObject.TransformChanged -= OnTransformChanged;
        }

        boundObject = levelObject;
        isDirty = true;

        if (boundObject != null)
        {
            boundObject.TransformChanged += OnTransformChanged;
        }

        if (isActive)
        {
            RefreshFromTransform();
        }
    }

    public void Clear()
    {
        if (boundObject != null)
        {
            boundObject.TransformChanged -= OnTransformChanged;
        }

        boundObject = null;
        isDirty = false;
    }

    public void OnActivated()
    {
        isActive = true;

        if (isDirty)
        {
            RefreshFromTransform();
        }
    }

    public void OnDeactivated()
    {
        isActive = false;
    }

    public void OnPositionEdited()
    {
        if (suppressCallbacks || boundObject == null)
        {
            return;
        }

        if (!TryParseVector3(positionX, positionY, positionZ, out Vector3 uiPosition))
        {
            return;
        }    

        Vector3 unityPosition = TransformUiAdapter.UiToUnityPosition(uiPosition);
        boundObject.SetPosition(unityPosition, createUndo: true);

        suppressCallbacks = true;
        NormalizeField(positionX, uiPosition.x, "F3");
        NormalizeField(positionY, uiPosition.y, "F3");
        NormalizeField(positionZ, uiPosition.z, "F3");
        suppressCallbacks = false;
    }

    private void OnRotationEdited()
    {
        if (suppressCallbacks || boundObject == null)
        {
            return;
        }

        if (!TryParseVector3(rotationX, rotationY, rotationZ, out Vector3 uiRotation))
        {
            return;
        }

        uiRotation.x = NormalizeAngle360(uiRotation.x);
        uiRotation.y = NormalizeAngle360(uiRotation.y);
        uiRotation.z = NormalizeAngle360(uiRotation.z);

        Vector3 unityRotation = TransformUiAdapter.UiToUnityRotation(uiRotation);
        boundObject.SetRotation(unityRotation, createUndo: true);

        suppressCallbacks = true;
        NormalizeField(rotationX, uiRotation.x, "F1");
        NormalizeField(rotationY, uiRotation.y, "F1");
        NormalizeField(rotationZ, uiRotation.z, "F1");
        suppressCallbacks = false;
    }

    private void OnScaleEdited()
    {
        if (suppressCallbacks || boundObject == null)
        {
            return;
        }

        if (!TryParseVector3(scaleX, scaleY, scaleZ, out Vector3 uiScale))
        {
            return;
        }

        uiScale.x = Mathf.Max(0.1f, uiScale.x);
        uiScale.y = Mathf.Max(0.1f, uiScale.y);
        uiScale.z = Mathf.Max(0.1f, uiScale.z);

        Vector3 unityScale = TransformUiAdapter.UiToUnityScale(uiScale);
        boundObject.SetScale(unityScale, createUndo: true);

        suppressCallbacks = true;
        NormalizeField(scaleX, uiScale.x, "F3");
        NormalizeField(scaleY, uiScale.y, "F3");
        NormalizeField(scaleZ, uiScale.z, "F3");
        suppressCallbacks = false;
    }

    private void OnTransformChanged(LevelObject _)
    {
        if (!isActive)
        {
            isDirty = true;
            return;
        }

        RefreshFromTransform();
    }

    private void RefreshFromTransform()
    {
        if (boundObject == null)
        {
            return;
        }

        suppressCallbacks = true;

        Transform boundObjectTransform = boundObject.transform;
        Vector3 uiPosition = TransformUiAdapter.UnityToUiPosition(boundObjectTransform.position);
        positionX.text = uiPosition.x.ToString("F3", Invariant);
        positionY.text = uiPosition.y.ToString("F3", Invariant);
        positionZ.text = uiPosition.z.ToString("F3", Invariant);

        Vector3 euler = boundObjectTransform.rotation.eulerAngles;
        Vector3 uiRotation = TransformUiAdapter.UnityToUiRotation(euler);
        rotationX.text = uiRotation.x.ToString("F1", Invariant);
        rotationY.text = uiRotation.y.ToString("F1", Invariant);
        rotationZ.text = uiRotation.z.ToString("F1", Invariant);

        Vector3 uiScale = TransformUiAdapter.UnityToUiScale(boundObjectTransform.localScale);
        scaleX.text = uiScale.x.ToString("F3", Invariant);
        scaleY.text = uiScale.y.ToString("F3", Invariant);
        scaleZ.text = uiScale.z.ToString("F3", Invariant);

        suppressCallbacks = false;
        isDirty = false;
    }

    private bool TryParseVector3(TMP_InputField x, TMP_InputField y, TMP_InputField z, out Vector3 result)
    {
        result = Vector3.zero;

        if (!float.TryParse(x.text, NumberStyles.Float, Invariant, out float fx) ||
            !float.TryParse(y.text, NumberStyles.Float, Invariant, out float fy) ||
            !float.TryParse(z.text, NumberStyles.Float, Invariant, out float fz))
        {
            Debug.LogWarning("[TransformInspectorTab] Failed to parse Vector3 input.");
            return false;
        }

        result = new Vector3(fx, fy, fz);
        return true;
    }

    private void NormalizeField(TMP_InputField field, float value, string format)
    {
        field.text = value.ToString(format, CultureInfo.InvariantCulture);
    }

    private static float NormalizeAngle360(float angle)
    {
        angle %= 360f;

        if (angle < 0f)
        {
            angle += 360f;
        }

        return angle;
    }

    private static string FilterFloatAllowNegative(string s) => NumericInputFilter.FilterFloat(s, true);
    private static string FilterFloatNoNegative(string s) => NumericInputFilter.FilterFloat(s, false);

    private void RegisterCallbacks()
    {
        RegisterNumericField(positionX, OnPositionEdited, FilterFloatAllowNegative);
        RegisterNumericField(positionY, OnPositionEdited, FilterFloatAllowNegative);
        RegisterNumericField(positionZ, OnPositionEdited, FilterFloatAllowNegative);

        RegisterNumericField(rotationX, OnRotationEdited, FilterFloatAllowNegative);
        RegisterNumericField(rotationY, OnRotationEdited, FilterFloatAllowNegative);
        RegisterNumericField(rotationZ, OnRotationEdited, FilterFloatAllowNegative);

        RegisterNumericField(scaleX, OnScaleEdited, FilterFloatNoNegative);
        RegisterNumericField(scaleY, OnScaleEdited, FilterFloatNoNegative);
        RegisterNumericField(scaleZ, OnScaleEdited, FilterFloatNoNegative);
    }

    private void RegisterNumericField(TMP_InputField field, UnityAction onEndEdit, System.Func<string, string> filter)
    {
        void valueChanged(string value)
        {
            if (suppressCallbacks)
            {
                return;
            }    

            string filtered = filter(value);
            if (filtered != value)
            {
                suppressCallbacks = true;
                field.text = filtered;
                suppressCallbacks = false;
            }
        }

        void endEdit(string _) => onEndEdit();

        field.onValueChanged.AddListener(valueChanged);
        field.onEndEdit.AddListener(endEdit);

        valueChangedHandlers[field] = valueChanged;
        endEditHandlers[field] = endEdit;
    }

    private void UnregisterCallbacks()
    {
        foreach (var keyValuePair in valueChangedHandlers)
        {
            if (keyValuePair.Key != null)
            {
                keyValuePair.Key.onValueChanged.RemoveListener(keyValuePair.Value);
            }
        }

        foreach (var keyValuePair in endEditHandlers)
        {
            if (keyValuePair.Key != null)
            {
                keyValuePair.Key.onEndEdit.RemoveListener(keyValuePair.Value);
            }
        }

        valueChangedHandlers.Clear();
        endEditHandlers.Clear();
    }
}
