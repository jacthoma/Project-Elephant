////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary> Class that creases a Catmull-Rom spline given a list of points. </summary>
public class CatmullRomMesh : MonoBehaviour
{
    List<Vector3> points = new List<Vector3>();
    List<SplineMeshSegment> segments = new List<SplineMeshSegment>();
    int _segmentCount = 0;

    int cylinderEdges = 16;
    
    /// <summary> The number of points the circumference of the circle will have.  The higher the number, the more "round" the spline will be.</summary>
    public int CylinderEdges
    {
        get { return cylinderEdges; }
        set
        {
            if (cylinderEdges != value)
            {
                cylinderEdges = value;
                UpdateAllSegments();
            }
        }
    }


    /// <summary> The number of extra cylinders per point.  The higher the number, the more "smooth" the spline will be.</summary>
    public int SamplesPerSegment
    {
        get { return samplesPerSegment; }
        set
        {
            if (samplesPerSegment != value && value >= 0)
            {
                samplesPerSegment = value;
                UpdateAllSegments();
            }
        }
    }
    int samplesPerSegment = 16;


    /// <summary> The layer of the GaneObjects in the spline.</summary>
    public int SplineLayer
    {
        get { return splineLayer; }
        set
        {
            if (splineLayer != value)
            {
                splineLayer = value;
                UpdateAllSegments();
            }
        }
    }
    int splineLayer = 1;


    /// <summary> The radius of the spline.</summary>
    public float SplineRadius
    {
        get { return splineRadius; }
        set
        {
            if (splineRadius != value)
            {
                splineRadius = value;
                UpdateAllSegments();
            }
        }
    }
    float splineRadius = 0.0018f;


    /// <summary> The current material of the spline.</summary>
    public Material CurrentMaterial
    {
        get { return currentMaterial; }
        set
        {
            currentMaterial = value;
            foreach (SplineMeshSegment segment in segments)
            {
                segment.SetMaterial(currentMaterial);
            }
        }
    }
    Material currentMaterial;
  
    /// <summary> The number of points in the list.</summary>
    public int PointCount { get { return points.Count; } }

    /// <summary> Adds a point to the end of the list. </summary>
    public void AddPoint(Vector3 point) { InsertPoint(points.Count, point); }

    /// <summary> Inserts a point into the list. </summary>
    public void InsertPoint(int index, Vector3 point)
    {
        //safe exit if index is invalid
        if (index > points.Count)
            return;

        points.Insert(index, point);
        //dont add a segment for the first point
        if (points.Count == 1)
        {
            return;
        }
        int segmentInsertPoint = (index >= 1) ? index - 1 : 0;
        segments.Insert(segmentInsertPoint, CreateNewSegment());
        UpdateSpline(index);
    }


    /// <summary> Modifies the position of a point. </summary>
    public void ModifyPoint(int index, Vector3 point)
    {
        //safe exit if index is invalid
        if (index >= points.Count)
            return;

        points[index] = point;
        UpdateSpline(index);
    }


    /// <summary> Removes a point at the given index from the list. </summary>
    public void RemovePoint(int index)
    {
        //safe exit if index is invalid
        if (index >= points.Count)
            return;

        points.RemoveAt(index);
        //if you removed the last point, then there are no segments
        if (points.Count == 0)
        {
            return;
        }
        int segmentRemovalPoint = (index >= 1) ? index - 1 : 0;
        SplineMeshSegment segmentToRemove = segments[segmentRemovalPoint];
        segments.RemoveAt(segmentRemovalPoint);
        Destroy(segmentToRemove.gameObject);
        UpdateSpline(index);
    }


    /// <summary> Returns the point at the given index. </summary>
    public Vector3 GetPoint(int index)
    {
        //safe exit if index is invalid
        if (index >= points.Count)
            return new Vector3(float.NaN, float.NaN, float.NaN);

        return points[index];
    }


    void UpdateSpline(int pointIndex)
    {
        UpdateSegment(pointIndex - 2);
        UpdateSegment(pointIndex - 1);
        UpdateSegment(pointIndex);
        UpdateSegment(pointIndex + 1);
    }


    /// <summary> Called when you change cylinder endges, samples per segment, or spline radius. </summary>
    void UpdateAllSegments()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            SplineMeshSegment sms = segments[i];
            sms.SamplesPerSegment = SamplesPerSegment;
            sms.CylinderEdges = CylinderEdges;
            sms.SplineRadius = SplineRadius;
            sms.gameObject.layer = SplineLayer;
            UpdateSegment(i);
        }
    }


    /// <summary> Updates a segment if it is at a valid location. </summary>
    void UpdateSegment(int segmentIndex)
    {
        //exit safely if index is out of range
        if (segmentIndex < 0 || segmentIndex >= segments.Count)
        {
            return;
        }
        SplineMeshSegment s = segments[segmentIndex];
        s.startingUpVector = GetUpVector(segmentIndex);
        s.SetupSegment(
            GetSafePointFromPointIndex(segmentIndex - 1),
            GetSafePointFromPointIndex(segmentIndex),
            GetSafePointFromPointIndex(segmentIndex + 1),
            GetSafePointFromPointIndex(segmentIndex + 2)
            );
    }


    Vector3 GetUpVector(int segmentIndex)
    {
        if (segmentIndex <= 0)
        {
            if (Vector3.Cross(points[0] - points[1], Vector3.up) == Vector3.zero)
            {
                return Vector3.right;
            }
            return Vector3.up;
        }
        return segments[segmentIndex - 1].finalUpVector;
    }


    /// <summary> Returns the point from the points list, but will ensure the index is always within range (so -1 returns points[0]). </summary>
    Vector3 GetSafePointFromPointIndex(int pointIndex)
    {
        if (pointIndex < 0)
        {
            return points[0];
        }
        if (pointIndex >= points.Count)
        {
            return points[points.Count - 1];
        }
        return points[pointIndex];
    }


    SplineMeshSegment CreateNewSegment()
    {
        GameObject segmentObject = new GameObject("Segment" + _segmentCount);
        segmentObject.transform.parent = this.transform;
        segmentObject.layer = SplineLayer;

        ++_segmentCount;

        SplineMeshSegment segment = segmentObject.AddComponent<SplineMeshSegment>();
        segment.SetMaterial(currentMaterial);
        segment.SamplesPerSegment = SamplesPerSegment;
        segment.CylinderEdges = CylinderEdges;
        segment.SplineRadius = SplineRadius;
        return segment;
    }
}