////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary> Class for holding onto the pieces that make up a spline.  Used by SplineDrawer.cs </summary>
public class Spline : MonoBehaviour
{
    /// <summary> The KnotList for the spline </summary>
    public KnotList knotList;

    /// <summary> The CatmullRomMesh for the spline </summary>
    public CatmullRomMesh splineMeshTool;

    void Awake()
    {
        knotList = gameObject.AddComponent<KnotList>();

        splineMeshTool = gameObject.AddComponent<CatmullRomMesh>();
        splineMeshTool.CylinderEdges = 16;
        splineMeshTool.SamplesPerSegment = 16;
        splineMeshTool.SplineLayer = 1;
        splineMeshTool.SplineRadius = 0.0045f;
    }
}