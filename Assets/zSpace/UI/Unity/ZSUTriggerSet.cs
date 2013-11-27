////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using zSpace.Common;
using zSpace.UI;

/// <summary>
/// Registers Unity-specific handlers with events in a Control.
/// </summary>
public class ZSUTriggerSet : ZSUMonoBehavior
{
    /// <summary>
    /// Associates a message in the Control with a handler function.
    /// </summary>
    protected struct Mapping
    {
        public string MessageName;
        public FrameworkMessageHandler Handler;
    }

    protected Mapping[] _mappings;
    protected bool _isSubscribed = false;

    public void AllHandler(FrameworkMessage message)
    {
        foreach (Mapping mapping in _mappings)
        {
            if (message.Message == mapping.MessageName)
                mapping.Handler(message);
        }
    }

    protected override void OnScriptUpdate()
    {
        if (!_isSubscribed)
        {
            ZSUVisualizerBase visualizer = GetComponent<ZSUVisualizerBase>();
            FrameworkControl control = visualizer.FrameworkControl;
            if (control != null)
            {
                if (visualizer.GetType() == typeof(ZSUModelVisualizer))
                    control = control.Parent;
     
                if (control != null)
                {
                    control.AddMessageHandler(this.AllHandler);
                    _isSubscribed = true;
                }
            }
        }

        base.OnScriptUpdate();
    }

    protected override void OnScriptDestroy()
    {
        if (_isSubscribed)
        {
            ZSUVisualizerBase visualizer = GetComponent<ZSUVisualizerBase>();
            FrameworkControl control = visualizer.FrameworkControl;
            if (control != null)
            {
                if (visualizer.GetType() == typeof(ZSUModelVisualizer))
                    control = control.Parent;
             
                if (control != null)
                {
                    control.RemoveMessageHandler(this.AllHandler);
                    _isSubscribed = false;
                }
            }
        }

        base.OnScriptDestroy();
    }
}
