////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;
using zSpace.UI.Unity;

/// <summary>
/// Unity interface for configuring a Control. Creates and associates a unity implementation, called a "Visualizer", with the Control.
/// </summary>
[UnityEngine.ExecuteInEditMode]
#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif
public class ZSUFrameworkControlProxy : ZSUMonoBehavior
{

    /// <summary>
    /// The default appearance set.
    /// </summary>
    public ZSUAppearanceSet AppearanceSet;

    /// <summary>
    /// Force visible layout size in Unity editor.
    /// </summary>
    public bool LayoutSizeVisible = false;

    /// <summary>
    /// If enabled, reveals the underlying visualizer node for this FrameworkControl.
    /// </summary>
    public bool ShowVisualizerInTree = false;

    /// <summary>
    /// The FrameworkControl tied to this proxy.
    /// </summary>
    public FrameworkControl FrameworkControl
    {
        get
        {
            return _nonSerialized == null ?
                null : _nonSerialized.FrameworkControl;
        }
    }

    /// <summary>
    /// Is this proxy currently the acting root for some FrameworkControl tree.
    /// </summary>
    public bool IsRoot
    {
        get
        {
            return DetectIsRoot();
        }
    }

    /// <summary>
    /// Is this a user-defined proxy or an automatically created proxy.
    /// </summary>
    public bool IsUserDefined
    {
        get
        {
            return _isUserDefinedProxy;
        }
    }


    static ZSUFrameworkControlProxy()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.update += OnEditorApplicationUpdate;
#endif
    }

    public void SetFrameworkControl(FrameworkControl newControl)
    {
        LoadSerializationBarrier();

        var existingControl = this._nonSerialized.FrameworkControl;
        if (existingControl == newControl)
        {
            return;
        }

        UnloadVisualizer(true, true);

        if (existingControl != null)
        {
            existingControl.UserObject = null;
        }

        this._nonSerialized.FrameworkControl = newControl;

        if (newControl == null)
        {
            return;
        }

        newControl.UserObject = this.gameObject;

        if (FrameworkControl.Visible)
        {
            LoadVisualizer(true);
        }
    }

    public void SetFrameworkControlType(string type, bool forceNew)
    {
        LoadSerializationBarrier();

        Type newType = null;
        Type existingType = this.FrameworkControl != null ? this.FrameworkControl.GetType() : null;

        if (!string.IsNullOrEmpty(type))
        {
            if (type.StartsWith("zSpace."))
            {
                newType = Utility.FindType(type);
            }
            else
            {
                newType = Utility.FindType("zSpace.UI." + type) ?? Utility.FindType("zSpace.UI.Utility." + type);
            }
        }

        if (newType != existingType || forceNew == true)
        {
            if (newType == null)
            {
                SetFrameworkControl(null);
            }
            else
            {
                FrameworkControl frameworkControl = (FrameworkControl)Activator.CreateInstance(newType);
                SetFrameworkControl(frameworkControl);
            }
        }
    }

    /// <summary>
    /// Saves the current FrameworkControl to Unity persistence.
    /// </summary>
    public void SaveFrameworkControl()
    {
        if (this.FrameworkControl == null || this._isUserDefinedProxy == false)
        {
            this._frameworkControlSerialized = null;
        }
        else
        {
            this._frameworkControlSerialized = FrameworkControlSerializer.Serialize(this.FrameworkControl);
        }
    }

    /// <summary>
    /// Loads the FrameworkControl from Unity persistence.
    /// Does not relink nodes in the FrameworkControl graph.
    /// </summary> 
    public void LoadFrameworkControl(bool forceReload)
    {
        LoadSerializationBarrier();

        if (this.FrameworkControl != null && forceReload == false)
        {
            return;
        }

        if (string.IsNullOrEmpty(this._frameworkControlSerialized))
        {
            this._nonSerialized.FrameworkControl = null;
        }
        else
        {
            FrameworkControl frameworkControl = FrameworkControlSerializer.Deserialize(this._frameworkControlSerialized);
            if (frameworkControl == null)
            {
                // could not deserialize. reset serialized data.
                Debug.LogWarning("Could not load saved data from proxy '" + this.name + "'. Data=\"" + this._frameworkControlSerialized + "\".");
            }
            SetFrameworkControl(frameworkControl);
        }
    }

    /// <summary>
    /// Invoked by proxy editor whenever the user
    /// has made modifications to the FrameworkControl.
    /// </summary>
    public void EditorDidChange()
    {
        _nonSerialized.IsDirtyFromEditor = true;
    }

    /// <summary>
    /// Invoked by ZSUModificationProcessor just prior
    /// to saving the scene file or creating a prefab.
    /// </summary>
    public void EditorPreSave()
    {
        UnloadNonUserChildProxies(true);
        UnloadVisualizer(true, true);
    }

    /// <summary>
    /// Invoked by ZSUModificationProcessor immediately
    /// after saving the scene file or creating a prefab.
    /// </summary>
    public void EditorDidSave()
    {
        if (this.IsRoot)
        {
            SynchronizeTree(this);
        }
    }

#if UNITY_EDITOR
    protected static void OnEditorApplicationUpdate()
    {
        if (_editorObjectsToDestroy.Count == 0)
        {
            return;
        }

        var objectsToDestroy = _editorObjectsToDestroy.ToList();
        _editorObjectsToDestroy.Clear();

        foreach (GameObject go in objectsToDestroy)
        {
            GameObject.DestroyImmediate(go);
        }
    }
#endif

    protected override void OnScriptAwake()
    {
        LoadSerializationBarrier();
        LoadFrameworkControl(true);
    }

    protected override void OnScriptStart()
    {
        if (this.IsRoot)
        {
            SynchronizeTree(this);
        }
    }

    protected override void OnScriptUpdate()
    {
        if (this.IsRoot)
        {
            SynchronizeTree(this);
        }
    }

    protected override void OnScriptDrawGizmos()
    {
        if (LayoutSizeVisible)
        {
            DrawLayoutSizeGizmo();
        }
    }

    protected override void OnScriptDrawGizmosSelected()
    {
        if (!LayoutSizeVisible)
        {
            DrawLayoutSizeGizmo();
        }
    }

    protected override void OnScriptDestroy()
    {
        base.OnScriptDestroy();

        if (this.FrameworkControl != null)
        {
            this.FrameworkControl.UserObject = null;
        }
    }

    protected override void OnEditorAwake()
    {
        LoadSerializationBarrier();
    }

    protected override void OnEditorEnable()
    {
        LoadSerializationBarrier();
        LoadFrameworkControl(false);

        var root = GetRootProxy();
        SynchronizeTree(root);
    }

    protected override void OnEditorUpdate()
    {
        if (_nonSerialized.IsDirtyFromEditor)
        {
            // Save.
            if (IsUserDefined)
            {
                SaveFrameworkControl();
            }

            // Do we need to reload the visualizer?
            {
                var visualizer = _visualizer;
                if (visualizer != null)
                {
                    // Change to ShowVisualizerInTree?
                    bool isHidden = (visualizer.hideFlags & HideFlags.HideInHierarchy) != 0;

                    // If should be visible but is hidden, or vice-versa.
                    if (ShowVisualizerInTree == isHidden)
                    {
                        LoadVisualizer(true);
                    }
                }
            }

            // Re-synchronize this tree.
            var root = GetRootProxy();
            SynchronizeTree(root);

            // Done.
            _nonSerialized.IsDirtyFromEditor = false;
        }
    }

    protected override void OnEditorDisable()
    {
        UnloadNonUserChildProxies();
        UnloadVisualizer(true);
    }

    private bool DetectIsRoot()
    {
        bool hasParentProxy =
            transform.parent != null
            && transform.parent.GetComponent<ZSUFrameworkControlProxy>() != null;
        bool isRoot = !hasParentProxy;
        return isRoot;
    }

    /// <summary>
    /// Finds the root proxy of this tree. Note that there
    /// is potentially more than one tree, thus potentially
    /// more than one root. 
    /// </summary>
    /// <returns></returns>
    private ZSUFrameworkControlProxy GetRootProxy()
    {
        Transform root = this.gameObject.transform;
        while (true)
        {
            var parent = root.parent;
            if (parent == null)
            {
                break;
            }
            if (parent.GetComponent<ZSUFrameworkControlProxy>() == null)
            {
                break;
            }
            root = parent;
        }

        return root.GetComponent<ZSUFrameworkControlProxy>();
    }

    private void UpdateControlTree(FrameworkControl control)
    {
        control.Update();
        foreach (FrameworkControl child in control.Children)
        {
            UpdateControlTree(child);
        }
    }

    private void SynchronizeTree(ZSUFrameworkControlProxy root)
    {
        root.LoadSerializationBarrier();
        if (root._nonSerialized.IsSynchronizing)
        {
            return;
        }

        try
        {
            root._nonSerialized.IsSynchronizing = true;
            FrameworkControl control = root.FrameworkControl;
            if (control != null)
            {
                UpdateControlTree(control); // r
                control.Layout(); // r
                root.SynchronizeUnityGraphStructure(); // r
                root.SynchronizeUnityTransforms(); // r
                root.SynchronizeUnityVisualizers(true); // r
            }
        }
        finally
        {
            root._nonSerialized.IsSynchronizing = false;
        }
    }

    private void DrawLayoutSizeGizmo()
    {
        // Draw a cube in the editor to represent our layout size.
        if (this.FrameworkControl != null)
        {
            // todo: Use pre-post-layout transform  instead.
            Gizmos.matrix = this.transform.localToWorldMatrix;
            Gizmos.color = FrameworkControl.IsVisible(true) ? BoundsColorVisible : BoundsColorInvisible;
            Gizmos.DrawWireCube(Vector3.zero, this.FrameworkControl.Size);
        }
    }

    /// <summary>
    /// Synchronize subgraph structure-only with Unity. (Recursive).
    /// </summary>
    private void SynchronizeUnityGraphStructure()
    {
        LinkedList<GameObject> childObjectsToDelete = new LinkedList<GameObject>();

        //
        // Inspect Unity graph for existing proxies.
        //
        foreach (Transform childGameObject in this.transform)
        {
            ZSUFrameworkControlProxy childProxy = childGameObject.GetComponent<ZSUFrameworkControlProxy>();
            if (childProxy != null)
            {
                FrameworkControl childControl = childProxy.FrameworkControl;

                // User proxy.
                if (childProxy._isUserDefinedProxy && childControl != null)
                {
                    if (childControl.Parent != this.FrameworkControl)
                    {
                        // Make sure this user proxy's object gets "adopted".
                        childControl.Parent = this.FrameworkControl;
                    }
                }
                // Generated proxy.
                else if (!childProxy._isUserDefinedProxy)
                {
                    if (childProxy.FrameworkControl != null)
                    {
                        if (this.FrameworkControl != childProxy.FrameworkControl.Parent)
                        {
                            // Orphaned proxy. Remove it.
                            childProxy.FrameworkControl.UserObject = null;
                            childObjectsToDelete.AddLast(childProxy.gameObject);
                        }
                    }
                    else
                    {
                        // Orphaned proxy. Remove it.
                        childObjectsToDelete.AddLast(childProxy.gameObject);
                    }
                }
            }
        }

        foreach (GameObject childToDelete in childObjectsToDelete)
        {
            UnityEngine.Object.DestroyImmediate(childToDelete);
        }
        childObjectsToDelete.Clear();


        //
        // Ensure control graph is represented in Unity graph.
        // 
        foreach (var childControl in this.FrameworkControl.Children)
        {
            // Ensure we have a game object for this child.
            GameObject childGameObject = childControl.UserObject as GameObject;
            if (childGameObject == null)
            {
                // note: The above expression uses Unity's overloaded operator==.
                // todo: Might want to attach behavior to each sub proxy so we can listen for OnDestroy. 

                string name = childControl.GetType().FullName;

                childGameObject = new GameObject(name);
                childGameObject.transform.parent = this.transform;
                childGameObject.layer = this.gameObject.layer;
                //childGameObject.hideFlags = HideFlags.DontSave;
                childGameObject.transform.localPosition = Vector3.zero;
                childGameObject.transform.localRotation = Quaternion.identity;
                childGameObject.transform.localScale = Vector3.one;
            }

            // Ensure we have a proxy on this child object.
            ZSUFrameworkControlProxy childProxy = childGameObject.GetComponent<ZSUFrameworkControlProxy>();
            if (childProxy == null)
            {
                childProxy = childGameObject.AddComponent<ZSUFrameworkControlProxy>();
                childProxy.AppearanceSet = this.AppearanceSet;
                childProxy.gameObject.layer = this.gameObject.layer;
                childProxy._isUserDefinedProxy = false;
            }

            childProxy.SetFrameworkControl(childControl);

            // Synchronize changes to appearance set.
            childProxy.AppearanceSet = this.AppearanceSet;
            childProxy.gameObject.layer = this.gameObject.layer;

            // Recurse.
            childProxy.SynchronizeUnityGraphStructure();
        }


        //
        // Update name to reflector descriptor.
        //
#if UNITY_EDITOR
        if (!this.IsUserDefined)
        {
            this.gameObject.name = this.FrameworkControl.GetVisualizationDescriptor().ToString(true);
        }
#endif
    }

    /// <summary>
    /// Destroy's any non-user proxies that were automatically
    /// created during synchronization of the framework graph
    /// to the Unity graph.
    /// </summary>
    /// <param name="forceImmediate"></param>
    private void UnloadNonUserChildProxies(bool forceImmediate = false)
    {
        LinkedList<GameObject> childObjectsToDelete = new LinkedList<GameObject>();

        //
        // Inspect Unity graph for existing proxies.
        //
        foreach (Transform childGameObject in this.transform)
        {
            ZSUFrameworkControlProxy childProxy = childGameObject.GetComponent<ZSUFrameworkControlProxy>();
            if (childProxy == null)
            {
                continue;
            }

            if (childProxy.IsUserDefined)
            {
                continue;
            }

            if (childProxy.FrameworkControl != null)
            {
                childProxy.FrameworkControl.UserObject = null;
            }
            childObjectsToDelete.AddLast(childProxy.gameObject);
        }

        foreach (GameObject childToDelete in childObjectsToDelete)
        {
            if (Application.isPlaying || forceImmediate)
            {
                UnityEngine.Object.DestroyImmediate(childToDelete);
            }
            else
            {
                DestroyObjectEditorSafe(childToDelete);
            }
        }
    }

    /// <summary>
    /// Synchronize subgraph transforms with Unity. (Recursive).
    /// </summary>
    private void SynchronizeUnityTransforms()
    {
        if (this.FrameworkControl != null)
        {
            if (!this.IsRoot)
            {
                this.transform.localPosition = this.FrameworkControl.FinalPosition;
                this.transform.localRotation = this.FrameworkControl.FinalRotation;
                this.transform.localScale = this.FrameworkControl.FinalScale;
            }
        }

        // Recurse.
        foreach (Transform child in this.transform)
        {
            ZSUFrameworkControlProxy childProxy = child.GetComponent<ZSUFrameworkControlProxy>();
            if (childProxy != null)
            {
                childProxy.SynchronizeUnityTransforms();
            }
        }
    }

    private void SynchronizeUnityVisualizers(bool recursive)
    {
        var visualizer = _visualizer;
        var control = this.FrameworkControl;

        if (control != null)
        {
            bool isVisible = control.IsVisible(true);
            if (isVisible && visualizer == null)
            {
                LoadVisualizer(false);
            }
            else if (!isVisible && visualizer != null)
            {
                UnloadVisualizer();
            }
        }

        if (visualizer != null && control != null)
        {
            visualizer.FrameworkControl = control;
            visualizer.Synchronize();
        }

        if (recursive)
        {
            foreach (Transform child in this.transform)
            {
                ZSUFrameworkControlProxy childProxy = child.GetComponent<ZSUFrameworkControlProxy>();
                if (childProxy != null)
                {
                    childProxy.SynchronizeUnityVisualizers(true);
                }
            }
        }
    }

    private void LoadSerializationBarrier()
    {
        if (this._nonSerialized == null)
        {
            this._nonSerialized = new SerializationBarrier();
        }
    }

    private void LoadVisualizer(bool reload)
    {
        var existingVisualizer = _visualizer;
        if (existingVisualizer != null)
        {
            if (reload)
            {
                UnloadVisualizer(true);
            }
            else
            {
                return;
            }
        }
        if (this.FrameworkControl == null || this.AppearanceSet == null)
        {
            return;
        }

        // Create visualizer.
        var visualizer = this.AppearanceSet.ResolveVisualizerAndAttach(this.FrameworkControl.GetVisualizationDescriptor(), this.gameObject);
        if (!this.ShowVisualizerInTree)
        {
            visualizer.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
        visualizer.FrameworkControl = this.FrameworkControl;

        // Remember
        this._visualizer = visualizer;

        // Synchronize once.
        visualizer.Synchronize();
    }

    private void UnloadVisualizer(bool cleanupOrphaned = false, bool forceImmediate = false)
    {
        var existingVisualizer = _visualizer;
        if (existingVisualizer != null)
        {
            try
            {
                if (Application.isPlaying || forceImmediate)
                {
                    GameObject.DestroyImmediate(existingVisualizer.gameObject);
                }
                else
                {
                    DestroyObjectEditorSafe(existingVisualizer.gameObject);
                }
            }
            catch { }
        }
        this._visualizer = null;

        if (cleanupOrphaned)
        {
            List<ZSUVisualizerBase> existingVisualizers
                = this.transform
                .Cast<Transform>()
                .Select(t => t.GetComponent<ZSUVisualizerBase>())
                .Where(v => v != null).ToList();

            foreach (var v in existingVisualizers)
            {
                if (Application.isPlaying || forceImmediate)
                {
                    GameObject.DestroyImmediate(v.gameObject);
                }
                else
                {
                    DestroyObjectEditorSafe(v.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// This is necessary for design-time scripts so that objects can be 
    /// deleted recursively, safely, when Unity may also be performing recursive 
    /// actions on the tree (such as recursively disabling nodes).
    /// </summary>
    /// <param name="unityGameObject"></param>
    private void DestroyObjectEditorSafe(GameObject unityGameObject)
    {
        if (!_editorObjectsToDestroy.Contains(unityGameObject))
        {
            _editorObjectsToDestroy.Add(unityGameObject);
        }
    }

    private readonly Color BoundsColorVisible = Color.yellow;
    private readonly Color BoundsColorInvisible = Color.gray;

    [HideInInspector]
    [SerializeField]
    public string _frameworkControlSerialized;
    [SerializeField]
    private bool _isUserDefinedProxy = true;
    [HideInInspector]
    public ZSUVisualizerBase _visualizer;

    private SerializationBarrier _nonSerialized;
    private class SerializationBarrier
    {
        public FrameworkControl FrameworkControl;
        public bool IsDirtyFromEditor;
        public bool IsSynchronizing;
    }

    /// <summary>
    /// Because we must cleanup our non-saved objects during OnDisable,
    /// but because Unity Editor breaks early with errors during recursive disabling
    /// if we remove children during that process, we must register our objects 
    /// for deletion, and manually remove them in the next Unity Editor update 
    /// callback.
    /// </summary>
    private static List<GameObject> _editorObjectsToDestroy = new List<GameObject>();
}

