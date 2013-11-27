////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

/// <summary> Class for adding and removing splines.  Used by ZSFreeformSplineTool. </summary>
public class FreeFormSplineDrawer : ISplineDrawer<FreeFormSpline>
{
    /// <summary> The distance the stylus needs to be from its last point to add a new point.</summary>
    public float DistanceThreshold = 0.1f;

    /// <summary> Called every frame you are dragging the stylus.  It updates the spline accordingly. </summary>
    public void UpdatePoint(Vector3 point)
    {
        CatmullRomMesh smt = currentlySelectedSpline.splineMeshTool;
        if(currentlySelectedSpline.splineMeshTool.PointCount == 0)
        {
            smt.AddPoint(point);
            smt.AddPoint(point);
            return;
        }
        smt.ModifyPoint(smt.PointCount - 1, point);
        if(Vector3.Distance(smt.GetPoint(smt.PointCount - 2), smt.GetPoint(smt.PointCount - 1)) > DistanceThreshold)
        {
            smt.AddPoint(point);
        }
    }


    /// <summary> Sets the material on the current spline. </summary>
    public void SetSplineMaterial(Material mat)
    {
        currentlySelectedSpline.splineMeshTool.CurrentMaterial = mat;
    }


    /// <summary> Sets the layer of all new GameObjects in the current spline. </summary>
    public void SetSplineLayer(int layer)
    {
        currentlySelectedSpline.splineMeshTool.SplineLayer = layer;
    }


    /// <summary> Sets the radius of the current spline. </summary>
    public void SetSplineRadius(float radius)
    {
        currentlySelectedSpline.splineMeshTool.SplineRadius = radius;
    }
    

    ///<summary>Gets an instance of the FreeFormSplineDrawer.  If one doesnt exist, it creates it.</summary>
    public static FreeFormSplineDrawer GetInstance()
    {
        if (_instance == null)
        {
            GameObject SplineManagerGameObject = new GameObject("Spline Freeform Drawing Manager");
            _instance = SplineManagerGameObject.AddComponent<FreeFormSplineDrawer>();
        }
        return _instance;
    }
    static FreeFormSplineDrawer _instance = null;
}
