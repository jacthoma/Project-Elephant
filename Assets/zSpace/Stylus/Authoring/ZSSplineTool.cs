////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Class for drawing a spline using knots </summary>
public class ZSSplineTool : ZSStylusTool
{
    /// <summary>
    /// If non-null, this prefab will be instantiated at each knot in the spline.
    /// </summary>
    public GameObject knotPrefab;

    /// <summary>
    /// If non-null, this prefab will be instantiated for the first knot in the spline.  Otherwise the default knot prefab will be used (if non-null).
    /// </summary>
    public GameObject firstKnotPrefab;

    /// <summary>
    /// The spline materials.  Each new spline will use the next material from this array.
    /// </summary>
    public Material[] splineMaterials;

    /// <summary>
    /// The spline radius.
    /// </summary>
    public float splineRadius = 0.0045f;

    /// <summary>
    /// The layer for all new spline GameObjects.
    /// </summary>
    public int splineLayer = 1;

    /// <summary>
    /// All splines will be placed under this object.  If null, one is created at the top level of the GameObject hierarchy.
    /// </summary>
    public GameObject _splineParent;

    /// <summary>
    /// When the stylus is within this distance of a knot, the knot will be selected.
    /// </summary>
    public float KnotSelectionThreshold = 0.01f;

    protected SplineDrawer _splineDrawer;
    protected int _materialId = 0;

    protected override void OnScriptStart()
    {
        base.OnScriptStart();

        ToolName = "ZSSplineTool";
        _splineDrawer = SplineDrawer.GetInstance();
        _splineDrawer._splineParent = _splineParent;
        _splineDrawer.AddSpline();
        _splineDrawer.SetFirstKnotPrefab(firstKnotPrefab);
        _splineDrawer.SetKnotPrefab(knotPrefab);
        _splineDrawer.SetSplineMaterial(splineMaterials[_materialId]);
        _splineDrawer.SetSplineLayer(splineLayer);
        _splineDrawer.SetSplineRadius(splineRadius);
    }


    public override void OnStylus()
    {
        base.OnStylus();

        ZSLinearShape linearShape = _stylusSelector.activeStylus as ZSLinearShape;
        Vector3 point = (_axisSnapResolution != Vector3.zero) ?
                            SnappedHoverPoint :
                            (linearShape != null) ?
                                linearShape._tip.transform.position :
                                _stylusSelector.activeStylus.hotSpot;

        if (_stylusSelector.GetButtonDown(0))
        {
            int splineId = _splineDrawer.GetCurrentSplineIndex();
            if (splineId == -1)
            {
              _splineDrawer.SelectNextSpline();
              _splineDrawer.SetFirstKnotPrefab(firstKnotPrefab);
              _splineDrawer.SetKnotPrefab(knotPrefab);
              _splineDrawer.SetSplineMaterial(splineMaterials[_materialId]);
              _splineDrawer.SetSplineLayer(splineLayer);
              _splineDrawer.SetSplineRadius(splineRadius);
            }

            bool pointSelected = _splineDrawer.SelectClosestKnot(point, KnotSelectionThreshold);
            bool isOnUi = _stylusSelector.HoverObject != null && _stylusSelector.HoverObject.layer == _stylusSelector.uiLayer;
            if (!isOnUi && !pointSelected)
            {
                _splineDrawer.AddKnot(point);
                _splineDrawer.SelectClosestKnot(point, KnotSelectionThreshold);

                _stylusSelector.useCollision = false;
            }
        }

        if (_stylusSelector.GetButtonUp(0))
        {
            _stylusSelector.useCollision = true;
            _splineDrawer.DeselectKnot();
        }

        _splineDrawer.UpdateSelectedKnot(point);

        if (_stylusSelector.GetButtonDown(1))
        {
            _splineDrawer.SelectNextSpline();
            _splineDrawer.SetFirstKnotPrefab(firstKnotPrefab);
            _splineDrawer.SetKnotPrefab(knotPrefab);
            _materialId = (_materialId + 1) % splineMaterials.Length;
            _splineDrawer.SetSplineMaterial(splineMaterials[_materialId]);
            _splineDrawer.SetSplineRadius(splineRadius);
        }

        if (_stylusSelector.GetButtonDown(2))
        {
            int splineId = _splineDrawer.GetCurrentSplineIndex();
            if (splineId == -1)
                _splineDrawer.SelectNextSpline();
            else
                _splineDrawer.SelectPreviousSpline();
                
            _splineDrawer.SetFirstKnotPrefab(firstKnotPrefab);
            _splineDrawer.SetKnotPrefab(knotPrefab);
            _materialId = (_materialId - 1) % splineMaterials.Length;
            _splineDrawer.SetSplineMaterial(splineMaterials[_materialId]);
            _splineDrawer.SetSplineRadius(splineRadius);
        }
    }
}
