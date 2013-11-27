////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;

/// <summary> Hides this framework control until the given target object is hovered. </summary>
public class ZSUToolTip : ZSUMonoBehavior
{
    /// <summary>The object which, when hovered, will activate this object's framework control.</summary>
    public ZSUFrameworkControlProxy TargetProxy;

    protected FrameworkControl _control;
    protected FrameworkControl _target;

    override protected void OnScriptStart()
    {
        base.OnScriptStart();

        _control = gameObject.GetComponent<ZSUFrameworkControlProxy>().FrameworkControl;
        _target = TargetProxy.FrameworkControl;
    }

    override protected void OnScriptUpdate()
    {
        base.OnScriptUpdate();

        //TODO: Trigger activation/deactivation animations.
        //TODO: Allow other types of events to cause tooltip to be shown.
        _control.Visible = _target.VisualizationState == "Hover";
    }
}
