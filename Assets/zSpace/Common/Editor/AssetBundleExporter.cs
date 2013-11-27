////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility for exporting selected assets as Unity AssetBundle files which can be loaded at runtime.
/// </summary>
/// <remarks>
/// Adds options to the Asets menu. One exports only the selected assets, while the other exports them along with all dependencies.
/// </remarks>
public class ExportAssetBundles
{
    [MenuItem("Assets/Build AssetBundle From Selection - Track dependencies")]
    static void ExportResource()
    {
        // Bring up save panel
        string path = EditorUtility.SaveFilePanel("Save Resource", "", "New Resource", "unity3d");
        if (path.Length != 0)
        {
            // Build the resource file from the active selection.
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
            Selection.objects = selection;
        }
    }

    [MenuItem("Assets/Build AssetBundle From Selection - No dependency tracking")]
    static void ExportResourceNoTrack()
    {
        // Bring up save panel
        string path = EditorUtility.SaveFilePanel("Save Resource", "", "New Resource", "unity3d");
        if (path.Length != 0)
        {
            // Build the resource file from the active selection.
            BuildPipeline.BuildAssetBundle(Selection.activeObject, Selection.objects, path);
        }
    }
}
