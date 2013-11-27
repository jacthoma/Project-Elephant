////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// The purpose of this class is to workaround
/// a Unity bug (the inability to save a scene with
/// GameObjects with NoSave flag), and also to ensure
/// that prefab creation does not store visualizers and
/// non-user nodes.
/// </summary>
/// <remarks>
/// See http://forum.unity3d.com/threads/8308-CheckConsistency-Transform-child-cant-be-loaded
/// for more information. 
/// </remarks>
[InitializeOnLoad]
public class ZSUModificationProcessor : AssetModificationProcessor
{
    static ZSUModificationProcessor()
    {
        EditorApplication.update += OnUpdateEditorApplication;
    }

    private static void OnUpdateEditorApplication()
    {
        bool wasSavingScene = _isSavingScene;
        bool wasCreatingPrefab = _isCreatingPrefab;

        if (!wasSavingScene && !wasCreatingPrefab)
        {
            return;
        }


        // This is (very likely?) the first editor update callback
        // since saving the scene or creating the prefab.


        if (wasSavingScene)
        {
            _isSavingScene = false;
        }

        if (wasCreatingPrefab)
        {
            _isCreatingPrefab = false;
        }

        NotifyDidSave();
    }

    public static void OnWillCreateAsset(string path)
    {
        bool isPrefab = Path.GetExtension(path).EndsWith(".prefab", System.StringComparison.InvariantCultureIgnoreCase);

        if (!isPrefab)
        {
            return;
        }

        _isCreatingPrefab = true;

        // Notify proxies.
        NotifyPreSave();
    }

    public static string[] OnWillSaveAssets(string[] paths)
    {
        // Determine whether we're saving a scene file or not.
        bool isSavingScene =
            paths
            .Select(p => Path.GetExtension(p))
            .Any(p => p.Equals(".unity", System.StringComparison.InvariantCultureIgnoreCase));

        if (!isSavingScene)
        {
            return paths;
        }

        _isSavingScene = true;

        // Notify proxies.
        NotifyPreSave();

        // Let Unity carry on its merry way.
        return paths;
    }

    private static void NotifyPreSave()
    {
        // Notify our active proxies that the scene 
        // file is about to be saved.
        var proxies =
            GameObject
            .FindObjectsOfType(typeof(ZSUFrameworkControlProxy))
            .Cast<ZSUFrameworkControlProxy>();
        foreach (var proxy in proxies)
        {
            proxy.EditorPreSave();
        }

    }

    private static void NotifyDidSave()
    {
        // Notify our active proxies that the
        // save process is finished.
        var proxies = GameObject.FindObjectsOfType(typeof(ZSUFrameworkControlProxy)).Cast<ZSUFrameworkControlProxy>();
        foreach (var proxy in proxies)
        {
            proxy.EditorDidSave();
        }
    }


    /// <summary>
    /// Are we between callbacks while Unity
    /// saves the scene file.
    /// </summary>
    private static bool _isSavingScene = false;

    /// <summary>
    /// Are we between callbacks while Unity
    /// is creating a prefab.
    /// </summary>
    private static bool _isCreatingPrefab = false;
}
