////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary> Class for the management of a list of knots.  Used with ZSSplineTool. </summary>
public class KnotList : MonoBehaviour
{
    /// <summary> The prefab that the first knot will be.  If null, no knots will be drawn. </summary>
    public GameObject firstKnotPrefab;

    /// <summary> The prefab that the knots will be.  If null, the default knot prefab will be used (if that's non-null). </summary>
    public GameObject knotPrefab;

    /// <summary> The number of knots in the list. </summary>
    public int KnotCount { get { return knots.Count; } }

    List<Knot> knots = new List<Knot>();

    int _knotCount = 0;


    /// <summary> Adds a knot to the end of the list. </summary>
    public void AddKnot(Vector3 point)
    {
        InsertKnot(knots.Count, point);
    }


    /// <summary> Inserts a knot at the given index in the list. </summary>
    public void InsertKnot(int index, Vector3 point)
    {
        knots.Insert(index, CreateKnot(point));
    }


    /// <summary> Modify the position of the knot at the given index. </summary>
    public void ModifyKnot(int index, Vector3 point)
    {
        knots[index].transform.position = point;
    }


    /// <summary> Remove a knot at the given index from the list. </summary>
    public void RemoveKnot(int index)
    {
        Knot knotToRemove = knots[index];
        knots.RemoveAt(index);
        Destroy(knotToRemove.gameObject);
    }


    /// <summary> 
    /// Returns the index of the closest knot to the passed in point.
    /// Only returns the point if it is within �threshold� distance of the point.
    /// Returns -1 if no knot meets the criteria.
    /// </summary>
    public int GetClosestKnotIndex(Vector3 point, float threshold)
    {
        int closestKnotIndex = -1;
        float closestKnotDistance = threshold;
        for (int i = 0; i < knots.Count; i++)
        {
            float distance = Vector3.Distance(point, knots[i].transform.position);
            if (distance <= closestKnotDistance)
            {
                closestKnotIndex = i;
            }
        }
        return closestKnotIndex;
    }


    /// <summary> Returns the position of the knot at the given index. </summary>
    public Vector3 GetKnot(int index)
    {
        return knots[index].transform.position;
    }


    Knot CreateKnot(Vector3 point)
    {
        GameObject prefab = (_knotCount == 0 && firstKnotPrefab != null) ? firstKnotPrefab : knotPrefab;
        GameObject knotGA = (prefab != null) ? Instantiate(prefab) as GameObject : new GameObject();
        knotGA.name = "Knot" + _knotCount;
        knotGA.transform.parent = this.transform;
        knotGA.layer = this.gameObject.layer;

        ++_knotCount;

        Knot knot = knotGA.AddComponent<Knot>();
        knot.transform.position = point;

        //TODO: Assumes the only call site is the one above.
        if (knots.Count > 0)
          knot.transform.rotation = Quaternion.LookRotation(point - knots[knots.Count-1].transform.position);

        return knot;
    }
}