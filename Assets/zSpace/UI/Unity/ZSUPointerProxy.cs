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

/// <summary>
/// Responsible for managing an instance of Pointer, performing
/// collision detection against FrameworkControls, and
/// notifying FrameworkControls of Pointer-based events.
/// </summary>
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class ZSUPointerProxy : ZSUMonoBehavior
{
    public ZSUIStylusInput StylusInput;

    protected override void OnScriptAwake()
    {
        const int buttonCount = 3; // hack: assumes 3 buttons.
        this._pointer = new Pointer(buttonCount);
    }

    protected override void OnScriptUpdate()
    {
        base.OnScriptUpdate();

        if (StylusInput == null)
        {
            return;
        }

        
        //
        // Check for proxies that were hidden.
        //

        // This process is necessary because Unity does not send trigger exit events
        // when the collider in question is removed from the scene.
        for (int i = 0; i < _enteredControlProxies.Count; ++i)
        {
            var proxy = _enteredControlProxies[i];
            var control = proxy.FrameworkControl;
            if (control == null || !control.IsVisible(true))
            {
                _enteredControlProxies.RemoveAt(i);
                --i;

                if (control != null)
                {
                    Vector3 positionLocal = proxy.transform.InverseTransformPoint(_pointer.Position);
                    PointerMessage pointerMessage = new PointerMessage(null, "PointerExited", this._pointer, positionLocal);
                    control.NotifyPointerExited(pointerMessage);
                }
            }
        }


        //
        // Update pointer object.
        //
        _pointer.Position = StylusInput.HoverPoint;
        _pointer.Direction = StylusInput.Direction;
        for (int i = 0; i < _pointer.ButtonCount; ++i)
        {
            _pointer.ButtonStates[i] = StylusInput.GetButton(i);
        }


        // 
        // Move proxy
        //
        this.transform.rotation = StylusInput.transform.rotation;
        if (this.StylusInput.HoverPoint.IsFinite())
        {
            this.transform.position = StylusInput.HoverPoint;
        }
        else
        {
            this.transform.position = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            _enteredControlProxies.Clear();
            return;
        }

        //
        // Send move messages
        //
        if (_enteredControlProxies.Any())
        {
            foreach (ZSUFrameworkControlProxy proxy in _enteredControlProxies.AsEnumerable().Reverse())
            {
                FrameworkControl control = proxy.FrameworkControl;
                if (control != null)
                {
                    Vector3 positionLocal = proxy.transform.InverseTransformPoint(_pointer.Position);
                    PointerMessage pointerMessage = new PointerMessage(null, "PointerMoved", _pointer, positionLocal);
                    control.NotifyPointerMoved(pointerMessage);

                    if (StylusInput.GetButton(0))
                    {
                        PointerMessage dragMessage = new PointerMessage(null, "PointerDragged", _pointer, positionLocal);
                        control.NotifyPointerDragged(dragMessage);
                    }
                }
            }
        }

        //
        // Detect button events.
        //
        if (StylusInput.GetButtonDown(0))
        {
            // notify currently entered proxies of the click.
            foreach (ZSUFrameworkControlProxy proxy in _enteredControlProxies.AsEnumerable().Reverse())
            {
                FrameworkControl control = proxy.FrameworkControl;
                if (control != null)
                {
                    Vector3 positionLocal = proxy.transform.InverseTransformPoint(_pointer.Position);
                    PointerMessage pointerMessage = new PointerMessage(null, "PointerActivated", _pointer, positionLocal);
                    control.NotifyPointerActivated(pointerMessage);
                }
            }
        }
    }

    protected override void OnScriptTriggerEnter(Collider otherCollider)
    {
        base.OnScriptTriggerEnter(otherCollider);

        // hack



        if (otherCollider.transform.parent == null)
        {
            return;
        }

        ZSUFrameworkControlProxy controlProxy = otherCollider.transform.parent.GetComponent<ZSUFrameworkControlProxy>();
        if (controlProxy != null)
        {
            if (_enteredControlProxies.Contains(controlProxy))
            {
                // fixme: duplicate enters are occurring under some circumstances.
                // For now just prevent adding duplicates to the list.

                return;
            }

            _enteredControlProxies.Add(controlProxy);

            FrameworkControl control = controlProxy.FrameworkControl;
            if (control != null)
            {
                Vector3 positionLocal = controlProxy.transform.InverseTransformPoint(_pointer.Position);
                PointerMessage pointerMessage = new PointerMessage(null, "PointerEntered", _pointer, positionLocal);
                control.NotifyPointerEntered(pointerMessage);
            }
        }
    }

    protected override void OnScriptTriggerExit(Collider otherCollider)
    {
        base.OnScriptTriggerExit(otherCollider);

        // hack



        if (otherCollider.transform.parent == null)
        {
            return;
        }

        ZSUFrameworkControlProxy controlProxy = otherCollider.transform.parent.GetComponent<ZSUFrameworkControlProxy>();
        if (controlProxy != null)
        {
            _enteredControlProxies.Remove(controlProxy);

            FrameworkControl control = controlProxy.FrameworkControl;
            if (control != null)
            {
                Vector3 positionLocal = controlProxy.transform.InverseTransformPoint(_pointer.Position);
                PointerMessage pointerMessage = new PointerMessage(null, "PointerExited", this._pointer, positionLocal);
                control.NotifyPointerExited(pointerMessage);
            }
        }
    }

    private Pointer _pointer;
    private List<ZSUFrameworkControlProxy> _enteredControlProxies = new List<ZSUFrameworkControlProxy>();
}
