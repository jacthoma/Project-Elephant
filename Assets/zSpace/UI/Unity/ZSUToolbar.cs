////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;
using zSpace.UI.Utility;

/// <summary>
/// Compound control with a set of buttons arranged in a pattern such as a row on the screen.
/// It can be hidden and exposed with an animation.
/// </summary>
public class ZSUToolbar : MonoBehaviour
{
    /// <summary>
    /// Maps a control in the toolbar to the function that will handle its messages
    /// and the next control (if any) that will replace it when the handler fires.
    /// </summary>
    [System.Serializable]
    public class Mapping
    {
        public ZSUFrameworkControlProxy Control;
        public ZSUFrameworkControlProxy NextControl;
        public KeyCode ShortcutKey;
        public string HandlerName;
        
        public FrameworkMessageHandler Handler { get; set; }
    }
    public Mapping[] Mappings = new Mapping[0];
    
    /// <summary>
    /// The script containing all the handlers for toolbar controls' messages.
    /// </summary>
    public MonoBehaviour TargetScript;
    
    /// <summary>
    /// An (optional) title for the toolbar.
    /// </summary>
    public string Title = string.Empty;
    
    public void Start()
    {
        if (this.GetComponent<ZSUFrameworkControlProxy>() != null)
        {
            Window window = this.GetComponent<ZSUFrameworkControlProxy>().FrameworkControl as Window;
            if (window != null)
                window.Title = Title;
        }
        
        foreach (Mapping mapping in Mappings)
        {
            FrameworkControl control = mapping.Control.FrameworkControl;
            FrameworkControl nextControl = (mapping.NextControl != null) ? mapping.NextControl.FrameworkControl : null;
            
            // Wrap the target function to do some additional UI maintenance.
            Type targetType = TargetScript.GetType();
            MethodInfo methodInfo = targetType.GetMethod(mapping.HandlerName);
            mapping.Handler = (FrameworkMessage message) =>
            {
                // Switch the control to the next version if applicable.
                if (nextControl != null)
                {
                    control.Visible = false;
                    nextControl.Visible = true;
                }
                
                // Invoke the specified function on the target.
                if (methodInfo != null)
                    methodInfo.Invoke(TargetScript, new object[] {message});
            };

            switch (control.GetType().Name)
            {
            case "Button":
                    ((Button)control).Activated += mapping.Handler;
                break;
            case "Slider":
                if (methodInfo != null)
                    ((Slider)control).Moved += mapping.Handler;
                break;
            default:
                Debug.Log("Unsupported control type in toolbar: " + control.GetType().Name);
                break;
            }
        }
    }
    
    void Update()
    {
        bool[] wasControlVisible = new bool[Mappings.Length];
        for (int i = 0; i < Mappings.Length; ++i)
        {
            if (Mappings[i].Control != null)
                wasControlVisible[i] = Mappings[i].Control.FrameworkControl.Visible;
        }
            
        for (int i = 0; i < Mappings.Length; ++i)
        {
            if (wasControlVisible[i] &&
                Mappings[i].ShortcutKey != KeyCode.None &&
                Input.GetKeyDown(Mappings[i].ShortcutKey))
                Mappings[i].Handler(null);
        }
    }
}
