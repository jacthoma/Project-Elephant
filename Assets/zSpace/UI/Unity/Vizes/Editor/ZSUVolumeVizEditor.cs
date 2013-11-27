////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor for the ZSUVolumeViz class.
/// </summary>
[CustomEditor(typeof(ZSUVolumeViz))]
public class ZSUVolumeVizEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        _viz = (ZSUVolumeViz)this.target;

        _viz.ValueMapPoint0 = EditorGUILayout.Vector4Field("Point 0", _viz.ValueMapPoint0);
        _viz.ValueMapPoint1 = EditorGUILayout.Vector4Field("Point 1", _viz.ValueMapPoint1);
        _viz.ValueMapPoint2 = EditorGUILayout.Vector4Field("Point 2", _viz.ValueMapPoint2);
        _viz.ValueMapPoint3 = EditorGUILayout.Vector4Field("Point 3", _viz.ValueMapPoint3);

        _viz.ValueMap = (ZSUVolumeViz.ValueMapping)EditorGUILayout.Popup("Value Map", (int)_viz.ValueMap, Enum.GetNames(typeof(ZSUVolumeViz.ValueMapping)));

        EditorUtility.SetDirty(_viz);
    }
        
    ZSUVolumeViz _viz;
}
