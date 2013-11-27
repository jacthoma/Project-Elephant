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

/// <summary> Class for drawing a spline using knots </summary>
public class ZSCrossSectionTool : ZSStylusTool
{
    /// <summary>
    /// This object will be transformed to match the current pose of the cutting plane.
    /// </summary>
    public GameObject Visualization;

    protected override void OnScriptAwake()
    {
        base.OnScriptAwake();

        Reset();
    }

    public void Reset()
    {
        SetPlane(1000f * Vector3.back, Vector3.back);
    }

    protected override void OnScriptStart()
    {
        base.OnScriptStart();

        ToolName = "ZSCrossSectionTool";
    }


    public override void OnStylus()
    {
        base.OnStylus();

        if (_stylusSelector.HoverObject != null && _stylusSelector.HoverObject.layer == _stylusSelector.uiLayer)
            return;

        // Set the world-space plane that will be cut by the shader.

        bool isButton = _toolButtons.Aggregate(false, (isPressed, buttonId) => isPressed |= _stylusSelector.GetButton(buttonId));
        if (isButton)
        {
            var linearShape = _stylusSelector.activeStylus as ZSLinearShape;
            var point = (linearShape != null && linearShape._tip != null) ? linearShape._tip.transform.position : _stylusSelector.HoverPoint;
            var normal = -_stylusSelector.Direction;
            var up = _stylusSelector.transform.up;
            SetPlane(point, normal, up);
        }
    }

    protected void SetPlane(Vector3 point, Vector3 normal)
    {
        SetPlane(point, normal, Vector3.up);
    }

    protected void SetPlane(Vector3 point, Vector3 normal, Vector3 up)
    {
        _crossSectionPlane = normal;
        _crossSectionPlane.w = Vector3.Dot(point, normal);
        Shader.SetGlobalVector("ZSCrossSectionPlane", _crossSectionPlane);

        if (Visualization != null)
        {
            Visualization.transform.position = point;
            Visualization.transform.rotation = Quaternion.LookRotation(normal, up);
        }
    }

    protected Vector4 _crossSectionPlane;
}
