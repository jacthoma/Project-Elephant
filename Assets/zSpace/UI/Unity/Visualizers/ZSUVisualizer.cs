////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using zSpace.UI;

/// <summary>
/// Base class for all Unity-based FrameworkControl visualizers.
/// </summary>
[UnityEngine.ExecuteInEditMode]
public class ZSUVisualizerBase : ZSUMonoBehavior
{
    public ZSUAppearanceSet AppearanceSet;
    public FrameworkControl FrameworkControl;

    public VisualizationDescriptor VisualizationDescriptor
    {
        get
        {
            return FrameworkControl.GetVisualizationDescriptor();
        }
    }

    protected override void OnScriptAwake()
    {
        base.OnScriptAwake();

        // Ensure collider is in 'trigger' mode.
        if (this.collider != null)
        {
            this.collider.isTrigger = true;
        }
    }

    protected override void OnEditorAwake()
    {
        base.OnEditorAwake();
    }

    /// <summary>
    /// Invoked on this object when it should synchronize its appearance 
    /// to the state of the given control.
    /// </summary>
    public virtual void Synchronize()
    {

    }
}

/// <summary>
/// Convenience subclass of ZSUVisualizer with strongly-typed access to the FrameworkControl.
/// </summary>
public class ZSUVisualizer<T> : ZSUVisualizerBase where T : zSpace.UI.FrameworkControl
{
    public new T FrameworkControl
    {
        get
        {
            return (T)base.FrameworkControl;
        }
    }
}
