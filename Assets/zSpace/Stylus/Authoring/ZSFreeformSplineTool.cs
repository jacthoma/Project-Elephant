////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ZSStylusShape))]

/// <summary> Class for drawing a spline by dragging the stylus </summary>
public class ZSFreeformSplineTool : ZSStylusTool
{
    /// <summary>
    /// The spline material.
    /// </summary>
    public Material splineMaterial;

    /// <summary>
    /// The layer for all new spline GameObjects.
    /// </summary>
    public int splineLayer = 1;

    /// <summary>
    /// The spline radius.
    /// </summary>
    public float splineRadius = 0.0015f;

    /// <summary>
    /// When the stylus is dragged further than this distance, a new control point will be added to the spline.
    /// </summary>
    public float DistanceThreshold = .002f;

    /// <summary>
    /// All splines will be placed under this object.  If null, one is created at the top level of the GameObject hierarchy.
    /// </summary>
    public GameObject _splineParent;

    FreeFormSplineDrawer _splineDrawer;

    protected override void OnScriptStart()
    {
        base.OnScriptStart();

        _splineDrawer = FreeFormSplineDrawer.GetInstance();
        _splineDrawer._splineParent = _splineParent;
    }

    public override void OnStylus()
    {
        base.OnStylus();

        _splineDrawer.DistanceThreshold = DistanceThreshold;

        if (_stylusSelector.GetButtonDown(0))
        {
            _splineDrawer.SelectNextSpline();
            _splineDrawer.SetSplineLayer(splineLayer);
            _splineDrawer.SetSplineRadius(splineRadius);
            _splineDrawer.SetSplineMaterial(splineMaterial);
        }

        if (_stylusSelector.GetButton(0))
            _splineDrawer.UpdatePoint(_stylusSelector.activeStylus.hotSpot);
    }
}
