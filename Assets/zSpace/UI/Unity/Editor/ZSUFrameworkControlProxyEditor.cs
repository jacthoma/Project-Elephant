////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;

/// <summary>
/// Unity editor interface for configuring a Control via a ZSUFrameworkControlProxy.
/// </summary>
[CustomEditor(typeof(ZSUFrameworkControlProxy))]
public class ZSUFrameworkControlProxyEditor : Editor
{
    void OnEnable()
    {
        _availableTypes = FindControlTypes();

        RequireProxyLoaded();
    }

    public override void OnInspectorGUI()
    {
        RequireProxyLoaded();
        if (!_frameworkControlProxy.enabled || !_frameworkControlProxy.gameObject.active)
        {
            EditorGUILayout.LabelField("Proxy is disabled.");
            return;
        }

        EditorGUIUtility.LookLikeControls(120f);

        // Core editor fields.
        InspectorFields();

        // Enforce chosen object type.
        {
            string newTypeAsString = _desiredType;
            if (!string.IsNullOrEmpty(newTypeAsString))
            {
                _frameworkControlProxy.SetFrameworkControlType(newTypeAsString, false);
            }
        }

        // Object-dependent editor fields.
        if (_frameworkControlProxy.FrameworkControl != null)
        {
            FrameworkControl control = _frameworkControlProxy.FrameworkControl;

            _showFoldoutAppearance = EditorGUILayout.Foldout(_showFoldoutAppearance, "Appearance");
            if (_showFoldoutAppearance)
            {
                InspectorFieldsAppearance(control);
            }

            _showFoldoutSubclassFields = EditorGUILayout.Foldout(_showFoldoutSubclassFields, "Control Fields");
            if (_showFoldoutSubclassFields)
            {
                InspectorFieldsSubclassFields(control);
            }

            _showFoldoutLayoutAttributes = EditorGUILayout.Foldout(_showFoldoutLayoutAttributes, "Layout");
            if (_showFoldoutLayoutAttributes)
            {
                InspectorFieldsLayoutAttributes(control);
            }

            _showFoldoutTransformAttributes = EditorGUILayout.Foldout(_showFoldoutTransformAttributes, "Transform (Post-Layout)");
            if (_showFoldoutTransformAttributes)
            {
                InspectorFieldsTransformAttributes(control);
            }

            _showFoldoutDebugging = EditorGUILayout.Foldout(_showFoldoutDebugging, "Debug");
            if (_showFoldoutDebugging)
            {
                InspectorFieldsDebugging(_frameworkControlProxy.FrameworkControl);
            }
        }

        if (!EditorApplication.isPlaying && GUI.changed)
        {
            _frameworkControlProxy.EditorDidChange();
            EditorUtility.SetDirty(this.target);
        }
    }

    private void RequireProxyLoaded()
    {
        if (_frameworkControlProxy == null)
        {
            _frameworkControlProxy = this.target as ZSUFrameworkControlProxy;
        }

        if (_frameworkControlProxy.FrameworkControl != null)
        {
            _desiredType = _frameworkControlProxy.FrameworkControl.GetType().FullName;
        }
    }

    private void InspectorFields()
    {
        // User choice of control type.
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Control Type");
                //string existingTypeName = _frameworkControlProxy.FrameworkControl != null ? _frameworkControlProxy.FrameworkControl.GetType().FullName : string.Empty;
                string existingTypeName = _desiredType;
                int existingTypeIndex = _availableTypes.IndexOf(existingTypeName);
                if (existingTypeIndex < 0) { existingTypeIndex = 0; }
                int newTypeIndex = EditorGUILayout.Popup(existingTypeIndex, _availableTypes);
                _desiredType = _availableTypes[newTypeIndex];
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void InspectorFieldsAppearance(FrameworkControl control)
    {
        //
        // Appearance
        //

        // Visible
        {
            control.Visible = EditorGUILayout.Toggle("Visible", control.Visible);
        }

        // Visualization Class
        {
            control.VisualizationClass
                = EditorGUILayout.TextField("Vis. Class", control.VisualizationClass ?? string.Empty);
        }

        // Appearance set
        _frameworkControlProxy.AppearanceSet = (ZSUAppearanceSet)EditorGUILayout.ObjectField("Appearance Set", _frameworkControlProxy.AppearanceSet, typeof(ZSUAppearanceSet), true);
    }

    private void InspectorFieldsLayoutAttributes(FrameworkControl control)
    {
        //
        // Size.
        //
        {
            control.LayoutAttributes.Size
                = EditorGUILayout.Vector3Field("Layout Size", control.LayoutAttributes.Size, GUILayout.Width(200f));
        }


        //
        // Layout self. Layout children.
        //
        {
            control.LayoutEnabledChildren
                = EditorGUILayout.Toggle("Layout Children", control.LayoutEnabledChildren);
            control.LayoutAttributes.IsEnabledSelf
                = EditorGUILayout.Toggle("Layout Self", control.LayoutAttributes.IsEnabledSelf);
        }

        //
        // Layout Position.
        //
        if (!control.LayoutAttributes.IsEnabledSelf)
        {
            control.Position = EditorGUILayout.Vector3Field("Position", control.Position);
        }
        else
        {
            EditorGUILayout.Vector3Field("Position (Automatic)", control.Position);
        }


        //
        // If layout enabled on self.
        //
        if (control.LayoutAttributes.IsEnabledSelf)
        {
            // Alignment.
            control.LayoutAttributes.Alignment[0]
                = EditorPopupEnum("Alignment X", control.LayoutAttributes.Alignment[0]);
            control.LayoutAttributes.Alignment[1]
                = EditorPopupEnum("Alignment Y", control.LayoutAttributes.Alignment[1]);
            control.LayoutAttributes.Alignment[2]
                = EditorPopupEnum("Alignment Z", control.LayoutAttributes.Alignment[2]);

            // Margins.
            control.LayoutAttributes.MarginsNegative[0] = EditorGUILayout.FloatField("Margin X(-)", control.LayoutAttributes.MarginsNegative[0]);
            control.LayoutAttributes.MarginsPositive[0] = EditorGUILayout.FloatField("Margin X(+)", control.LayoutAttributes.MarginsPositive[0]);
            control.LayoutAttributes.MarginsNegative[1] = EditorGUILayout.FloatField("Margin Y(-)", control.LayoutAttributes.MarginsNegative[1]);
            control.LayoutAttributes.MarginsPositive[1] = EditorGUILayout.FloatField("Margin Y(+)", control.LayoutAttributes.MarginsPositive[1]);
            control.LayoutAttributes.MarginsNegative[2] = EditorGUILayout.FloatField("Margin Z(-)", control.LayoutAttributes.MarginsNegative[2]);
            control.LayoutAttributes.MarginsPositive[2] = EditorGUILayout.FloatField("Margin Z(+)", control.LayoutAttributes.MarginsPositive[2]);
        }

        // Show layout bounds in editor.
        _frameworkControlProxy.LayoutSizeVisible = EditorGUILayout.Toggle("Show Layout Size", _frameworkControlProxy.LayoutSizeVisible);
    }

    private void InspectorFieldsTransformAttributes(FrameworkControl control)
    {
        control.TransformAttributes.Translation = EditorGUILayout.Vector3Field("Translation", control.TransformAttributes.Translation);
        control.TransformAttributes.Rotation = EditorQuaternion("Rotation", control.TransformAttributes.Rotation);
        control.TransformAttributes.RotationOrigin = EditorGUILayout.Vector3Field("Rotation Origin", control.TransformAttributes.RotationOrigin);
        control.TransformAttributes.Scale = EditorGUILayout.Vector3Field("Scale", control.TransformAttributes.Scale);
    }

    private void InspectorFieldsSubclassFields(FrameworkControl control)
    {
        // Find member fields of FrameworkContorl that aren't already part of the base class.
        Type type = control.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        bool didShowField = false;
        foreach (FieldInfo field in fields)
        {
            if (field.DeclaringType == typeof(FrameworkControl))
            {
                continue;
            }

            // Generate UI for this field.
            InspectorFieldForFrameworkControlField(control, field);
            didShowField = true;
        }

        if (!didShowField)
        {
            EditorGUILayout.LabelField("(none)");
        }
    }

    private void InspectorFieldsDebugging(FrameworkControl control)
    {
        EditorGUILayout.Toggle("Is User Proxy (readonly)", _frameworkControlProxy.IsUserDefined);
        _frameworkControlProxy.ShowVisualizerInTree = EditorGUILayout.Toggle("Show Visualizer Node", _frameworkControlProxy.ShowVisualizerInTree);
        EditorGUILayout.LabelField("Serialized State");
        EditorGUILayout.TextArea((_frameworkControlProxy._frameworkControlSerialized ?? string.Empty).Trim(), GUILayout.Height(160));

        // Locate hidden children.
        EditorGUILayout.LabelField("Hidden Children");
        List<Transform> hiddenChildren
            = _frameworkControlProxy
            .transform
            .Cast<Transform>()
            .Where(t => t.hideFlags != 0)
            .ToList();

        if (hiddenChildren.Count == 0)
        {
            EditorGUILayout.LabelField(" (none) ");
        }
        else
        {
            foreach (var child in hiddenChildren)
            {
                EditorGUILayout.TextField(child.name);
            }
        }
    }

    private void InspectorFieldForFrameworkControlField(FrameworkControl control, FieldInfo field)
    {
        if (field.FieldType == typeof(string))
        {
            string value = EditorGUILayout.TextField(field.Name, (string)field.GetValue(control) ?? string.Empty);
            field.SetValue(control, value);
        }
        else if (field.FieldType == typeof(Vector3))
        {
            Vector3 value = EditorGUILayout.Vector3Field(field.Name, (Vector3)field.GetValue(control));
            field.SetValue(control, value);
        }
        else if (field.FieldType == typeof(bool))
        {
            bool value = EditorGUILayout.Toggle(field.Name, (bool)field.GetValue(control));
            field.SetValue(control, value);
        }
        else if (field.FieldType == typeof(int))
        {
            int value = EditorGUILayout.IntField(field.Name, (int)field.GetValue(control));
            field.SetValue(control, value);
        }
        else if (field.FieldType == typeof(float))
        {
            float value = EditorGUILayout.FloatField(field.Name, (float)field.GetValue(control));
            field.SetValue(control, value);
        }
        else if (field.FieldType == typeof(Quaternion))
        {
            Quaternion value = EditorQuaternion(field.Name, (Quaternion)field.GetValue(control));
            field.SetValue(control, value);
        }
        else if (field.FieldType.IsEnum)
        {
            object value = EditorPopupEnum(field.Name, field.GetValue(control));
            field.SetValue(control, value);
        }
        else
        {
            Debug.LogWarning("Cannot display editor field for member field '" + field.Name + "'.");
        }
    }

    private Quaternion EditorQuaternion(string label, Quaternion currentValue)
    {
        // todo: Make this a little more user friendly, on par with Unity's built-in rotation fields behavior.
        // It will likely need to persist additional state somewhere about user-entered XYZ.

        Quaternion value = currentValue;
        Vector3 valueAsEulerAngles = value.eulerAngles;
        Vector3 newValueAsEulerAngles = EditorGUILayout.Vector3Field(label, valueAsEulerAngles);
        if (valueAsEulerAngles != newValueAsEulerAngles)
        {
            // The above check is necessary to avoid numerical instability in the editor pane 
            // due to to conversions to-from euler angles.
            Quaternion newValue = Quaternion.Euler(newValueAsEulerAngles);
            return newValue;
        }
        else
        {
            return currentValue;
        }
    }

    /// <summary>
    /// Helper method to quickly generate an editor "popup" list based on an enumeration type T.
    /// </summary>
    /// <remarks>
    /// The generic method would ideally have constraint 'where T : Enum' except that Enum constraints are not supported in C#.
    /// </remarks>
    private object EditorPopupEnum(string label, object currentValue)
    {
        EditorGUILayout.BeginHorizontal();

        {
            EditorGUILayout.LabelField(label);
        }

        object newValue;
        {
            Type enumType = currentValue.GetType();
            if (!enumType.IsEnum)
            {
                Debug.LogError("Provided type not an enum.");
            }

            object[] enumValues = Enum.GetValues(enumType).Cast<object>().ToArray();
            string[] enumLabels = enumValues.Select(v => v.ToString()).ToArray();
            int[] intValues = enumValues.Select((v) => System.Convert.ToInt32(v)).ToArray();

            int currentValueAsInt = System.Convert.ToInt32(currentValue);
            int currentIndex = intValues.IndexOf(currentValueAsInt);
            int newIndex = EditorGUILayout.Popup(currentIndex, enumLabels);
            newValue = enumValues[newIndex];
        }

        EditorGUILayout.EndHorizontal();
        return newValue;
    }

    private T EditorPopupEnum<T>(string label, T currentValue)
    {
        return (T)EditorPopupEnum(label, (object)currentValue);
    }

    private string[] FindControlTypes()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(FrameworkControl));
        var controlTypes
            = assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(FrameworkControl)))
            .Where(t => !t.IsGenericType);
        return (new string[] { "(none)" }).Concat(controlTypes.Select(t => t.FullName)).ToArray();
    }


    private ZSUFrameworkControlProxy _frameworkControlProxy;
    private string _desiredType;
    private string[] _availableTypes;
    private bool _showFoldoutAppearance;
    private bool _showFoldoutSubclassFields;
    private bool _showFoldoutLayoutAttributes;
    private bool _showFoldoutTransformAttributes;
    private bool _showFoldoutDebugging;
}
