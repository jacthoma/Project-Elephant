////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;
using zSpace.UI.Utility;

/// <summary>
/// Rumbles the stylus when objects are hovered, selected, or acted upon with a tool.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ZSStylusRumbler : ZSUMonoBehavior
{
    /// <summary>
    /// A combination of sound and/or vibration which will be played in response to a particular event.
    /// </summary>
    [Serializable]
    public class Effect
    {
        public int RumbleStrength;
        public AudioClip Sound;
    }

    public Effect HoverEffect;
    public Effect UnhoverEffect;
    public Effect SelectEffect;
    public Effect DeselectEffect;
    public Effect ToolOnEffect;
    public Effect ToolOffEffect;
    public Effect SnapOnEffect;
    public Effect SnapOffEffect;
    public Effect CollideEffect;

    protected override void OnScriptAwake()
    {
        base.OnScriptAwake();

        _core = GameObject.FindObjectOfType(typeof(ZSCore)) as ZSCore;
        _stylusSelector = GameObject.FindObjectOfType(typeof(ZSStylusSelector)) as ZSStylusSelector;
    }


    protected override void OnScriptStart()
    {
        base.OnScriptStart();

        foreach (var proxy in GameObject.FindObjectsOfType(typeof(ZSUFrameworkControlProxy)) as ZSUFrameworkControlProxy[])
        {
            var button = proxy.FrameworkControl as Button;
            if (button != null)
                button.Activated += new FrameworkMessageHandler((message) => this.OnSelectBegin(null));
        }
    }

    protected override void OnScriptUpdate()
    {
        base.OnScriptUpdate();

        {
            GameObject hoverObject = _stylusSelector.HoverObject;
            if (hoverObject != _oldHoverObject)
            {
                if (hoverObject != null)
                    OnHoverBegin(hoverObject);
                else
                    OnHoverEnd(_oldHoverObject);
            }
            _oldHoverObject = hoverObject;
        }
        {
            GameObject[] selectedObjects = _stylusSelector.selectedObjects.ToArray();

            if (_oldSelectedObjects.Except(selectedObjects).Count() != 0)
                OnSelectEnd(null);

            if (selectedObjects.Except(_oldSelectedObjects).Count() != 0)
                OnSelectBegin(null);

            _oldSelectedObjects = selectedObjects;
        }
        {
            bool isToolActive = _stylusSelector.activeStylus.Tool.IsOperating;
    
            if (isToolActive && !_oldIsToolActive)
                OnToolBegin(null);
    
            if (!isToolActive && _oldIsToolActive)
                OnToolEnd(null);
    
            _oldIsToolActive = isToolActive;
        }
        {
            bool isSnapped = false;
            //TODO: Expensive!  Add events to Snap instead.
            Snap[] snaps = GameObject.FindObjectsOfType(typeof(Snap)) as Snap[];
            foreach (Snap snap in snaps)
                isSnapped |= (snap.mateObject != null);

            if (isSnapped && !_wasSnapped)
                OnSnapBegin(null);

            if (!isSnapped && _wasSnapped)
                OnSnapEnd(null);

            _wasSnapped = isSnapped;
        }
    }

    protected void OnHoverBegin(GameObject go) { PlayEffect(HoverEffect); }
    protected void OnHoverEnd(GameObject go) { PlayEffect(UnhoverEffect); }
    protected void OnSelectBegin(GameObject go) { PlayEffect(SelectEffect); }
    protected void OnSelectEnd(GameObject go) { PlayEffect(DeselectEffect); }
    protected void OnToolBegin(GameObject go) { PlayEffect(ToolOnEffect); }
    protected void OnToolEnd(GameObject go) { PlayEffect(ToolOffEffect); }
    protected void OnSnapBegin(GameObject go) { PlayEffect(SnapOnEffect); }
    protected void OnSnapEnd(GameObject go) { PlayEffect(SnapOffEffect); }
    protected void OnCollide(GameObject go) { PlayEffect(CollideEffect); }

    /// <summary>
    /// Initiates playback of the specified sound and/or vibration effect.
    /// </summary>
    public void PlayEffect(Effect effect)
    {
        if (effect.RumbleStrength != 0)
            Shake(effect.RumbleStrength);

        if (effect.Sound != null)
            audio.PlayOneShot(effect.Sound);
    }

    /// <summary>
    /// Uses dithering to vibrate the stylus at the given intensity.
    /// </summary>
    /// <remarks>
    /// If the stylus is already vibrating, this does nothing.
    /// </remarks>
    public void Shake(int intensity)
    {
        if (_isVibrating)
            return;

        float onPeriod = 0f;
        float offPeriod = 0f;
        int repeatCount = 0;

        switch (intensity)
        {
            case 0:
            break;
            case 1:
                onPeriod = 0.032f;
                offPeriod = 0.128f;
                repeatCount = 0;
            break;
            case 2:
                onPeriod = 0.032f;
                offPeriod = 0.064f;
                repeatCount = 0;
            break;
            case 3:
                onPeriod = 0.032f;
                offPeriod = 0.064f;
                repeatCount = 1;
            break;
            case 4:
                onPeriod = 0.064f;
                offPeriod = 0.128f;
                repeatCount = 0;
            break;
            case 5:
                onPeriod = 0.64f;
                offPeriod = 0.064f;
                repeatCount = 0;
            break;
            case 6:
                onPeriod = 0.64f;
                offPeriod = 0.064f;
                repeatCount = 1;
            break;
            case 7:
                onPeriod = 0.128f;
                offPeriod = 0.256f;
                repeatCount = 0;
            break;
            case 8:
                onPeriod = 0.128f;
                offPeriod = 0.128f;
                repeatCount = 0;
            break;
            case 9:
                onPeriod = 0.128f;
                offPeriod = 0.128f;
                repeatCount = 1;
            break;
            default:
                onPeriod = 0.1f * (float)intensity;
            break;
        }

        _core.SetStylusVibrationOnPeriod(onPeriod);
        _core.SetStylusVibrationOffPeriod(offPeriod);
        _core.SetStylusVibrationRepeatCount(repeatCount);
        _core.SetStylusVibrationEnabled(true);
        _core.StartStylusVibration();
        _isVibrating = true;

        float waitTime = (onPeriod + offPeriod) * (repeatCount + 1);
        StartCoroutine(Utility.Delay(waitTime, () => {_isVibrating = false;}));
    }

    protected ZSCore _core;
    protected ZSStylusSelector _stylusSelector;

    protected GameObject _oldHoverObject;
    protected GameObject[] _oldSelectedObjects = new GameObject[] {};
    protected bool _oldIsToolActive = false;
    protected bool _wasSnapped = false;
    protected bool _isVibrating = false;
}
