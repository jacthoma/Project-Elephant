////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

/// <summary> Class for holding onto the pieces that make up a spline.  Used by SplineDrawingFreeformManager.cs </summary>
public class FreeFormSpline : MonoBehaviour
{
    /// <summary> The CatmullRomMesh for the spline </summary>
    public CatmullRomMesh splineMeshTool;

    void Awake()
    {
        splineMeshTool = gameObject.AddComponent<CatmullRomMesh>();
        splineMeshTool.CylinderEdges = 8;
        splineMeshTool.SamplesPerSegment = 0;
        splineMeshTool.SplineLayer = 1;
        splineMeshTool.SplineRadius = .0015f;
    }
}