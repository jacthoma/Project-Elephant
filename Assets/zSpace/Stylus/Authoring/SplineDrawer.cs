////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary> Class for adding and removing splines.  Used by ZSSplineTool. </summary>
public class SplineDrawer : ISplineDrawer<Spline>
{
    public int CurrentlySelectedKnotIndex { get { return currentlySelectedKnotIndex; } }
    int currentlySelectedKnotIndex = -1;

    /// <summary> The number of splines. </summary>
    public int CurrentSplineKnotCount { get { return currentlySelectedSpline.knotList.KnotCount; } }

    /// <summary> Adds a knot at the end of the current spline. </summary>
    public void AddKnot(Vector3 point)
    {
        currentlySelectedSpline.knotList.AddKnot(point);
        currentlySelectedSpline.splineMeshTool.AddPoint(point);
        DeselectKnot();
    }


    /// <summary> Inserts a knot at the given index of the current spline. </summary>
    public void InsertKnot(int index, Vector3 point)
    {
        currentlySelectedSpline.knotList.InsertKnot(index, point);
        currentlySelectedSpline.splineMeshTool.InsertPoint(index, point);
        DeselectKnot();
    }


    /// <summary> Returns the knot position at the given index of the current spline. </summary>
    public Vector3 GetKnotPosition(int index)
    {
        return currentlySelectedSpline.knotList.GetKnot(index);
    }


    /// <summary> Modifies the knot at the given index of the current spline. </summary>
    public void ModifyKnot(int index, Vector3 point)
    {
        currentlySelectedSpline.knotList.ModifyKnot(index, point);
        currentlySelectedSpline.splineMeshTool.ModifyPoint(index, point);
    }


    /// <summary> Removes the knot at the given index of the current spline. </summary>
    public void RemoveKnot(int index)
    {
        currentlySelectedSpline.knotList.RemoveKnot(index);
        currentlySelectedSpline.splineMeshTool.RemovePoint(index);
        DeselectKnot();
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
        currentlySelectedSpline.knotList.gameObject.layer = layer;
    }


    /// <summary> Sets the radius of the current spline. </summary>
    public void SetSplineRadius(float radius)
    {
        currentlySelectedSpline.splineMeshTool.SplineRadius = radius;
    }


    /// <summary> Sets the prefab that will be used the first knot on the currently selected spline. </summary>
    public void SetFirstKnotPrefab(GameObject go)
    {
        currentlySelectedSpline.knotList.firstKnotPrefab = go;
    }


    /// <summary> Sets the prefab that will be used for all new knots on the currently selected spline. </summary>
    public void SetKnotPrefab(GameObject go)
    {
        currentlySelectedSpline.knotList.knotPrefab = go;
    }


    ///<summary>
    /// Selects the closest knot to the given point if it is within 'threshold' distance.
    /// Returns true if a knot was selected.
    /// </summary>
    public bool SelectClosestKnot(Vector3 point, float threshold, bool doCheckAllSplines = false)
    {
        int closestSplineId = -1;
        int closestKnotId = -1;
        float minKnotDistance = threshold;

        Spline[] splinesToCheck = (doCheckAllSplines) ? splines.ToArray() : new Spline[] { currentlySelectedSpline };

        foreach (Spline spline in splinesToCheck)
        {
            if (spline == null)
                continue;

            int knotId = spline.knotList.GetClosestKnotIndex(point, minKnotDistance);
            if (knotId >= 0)
            {
                closestSplineId = splines.IndexOf(spline);
                closestKnotId = knotId;
            }
        }

        bool result = (closestKnotId != -1);
        if (result)
        {
            currentlySelectedSpline = splines[closestSplineId];
            currentlySelectedKnotIndex = closestKnotId;
        }

        return result;
    }


    ///<summary> Deselects the currently selected knot. </summary>
    public void DeselectKnot()
    {
        currentlySelectedKnotIndex = -1;
    }


    ///<summary> Moves the selected knot to the given position. </summary>
    public void UpdateSelectedKnot(Vector3 point)
    {
        if (currentlySelectedKnotIndex != -1)
        {
            ModifyKnot(currentlySelectedKnotIndex, point);
        }
    }


    ///<summary> Gets an instance of the SplineDrawer.  If one doesn't exist, it is created. </summary>
    public static SplineDrawer GetInstance()
    {
        if (_instance == null)
        {
            GameObject SplineManagerGameObject = new GameObject("Spline Drawer");
            _instance = SplineManagerGameObject.AddComponent<SplineDrawer>();
        }
        return _instance;
    }
    static SplineDrawer _instance = null;
}