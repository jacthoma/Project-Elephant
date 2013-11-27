////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;
using zSpace.UI.Utility;

/// <summary>
/// Internal class for representing a Plane (part of a Control).
/// </summary>
public class ZSUPlaneVisualizer : ZSUVisualizer<zSpace.UI.Utility.Plane>
{
    /// <summary>
    /// The material for the appearance of the plane.
    /// </summary>
    public Material Material;

    public override void Synchronize()
    {
        base.Synchronize();

        if (_backgroundPlane == null)
        {
            _backgroundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);

            _backgroundPlane.transform.parent = this.transform;
            _backgroundPlane.layer = this.gameObject.layer;
            _backgroundPlane.transform.localScale = Vector3.one;
            _backgroundPlane.transform.localPosition = Vector3.zero;
            _backgroundPlane.transform.localRotation = Quaternion.LookRotation(Vector3.up, Vector3.back);
            _backgroundPlane.name = "Plane";
        }

        _backgroundPlane.GetComponent<MeshRenderer>().sharedMaterial = this.Material;
        Vector3 planeSize = this.FrameworkControl.FinalSize;
        Vector3 planeScale = planeSize.SwizzleXZY() * 0.1f;
        Vector3 planeOffset = Vector3.zero;

        _backgroundPlane.transform.localScale = planeScale;
        _backgroundPlane.transform.localPosition = planeOffset;
    }


    private GameObject _backgroundPlane;
}

