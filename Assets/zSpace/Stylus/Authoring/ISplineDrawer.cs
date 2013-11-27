////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary> An abstract class from which spline drawers inherit.
/// In charge of basic list manipulation that are performed on the list of splines 
/// Keeps track of a currently selected spline.</summary>
public abstract class ISplineDrawer<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// All splines will be placed under this object.  If null, one is created at the top level of the GameObject hierarchy.
    /// </summary>
    public GameObject _splineParent;

    protected List<T> splines = new List<T>();
    protected T currentlySelectedSpline;
    static int _splineCount = 0;

    /// <summary> The total number of splines.</summary>
    public int SplineCount { get { return splines.Count; } }


    /// <summary>Returns the index of the currently selected spline.</summary>
    public int GetCurrentSplineIndex()
    {
        if (currentlySelectedSpline == null)
        {
            return -1;
        }
        int index = splines.IndexOf(currentlySelectedSpline);
        return index;
    }


    /// <summary>Selects the next spline in the list</summary>
    public void SelectNextSpline()
    {
        int splineId = GetCurrentSplineIndex();
        if (splineId == -1 || splineId + 1 >= SplineCount)
        {
            AddSpline();
        }
        else
        {
            int index = splines.IndexOf(currentlySelectedSpline);
            SelectSpline(index + 1);
        }
    }


    /// <summary>Selects the previous spline in the list</summary>
    public void SelectPreviousSpline()
    {
        int index = splines.IndexOf(currentlySelectedSpline);
        SelectSpline((index - 1 + splines.Count) % splines.Count);
    }


    /// <summary>Selects the spline at the given index</summary>
    public void SelectSpline(int index)
    {
        if (index < 0 || index >= splines.Count)
        {
            return;
        }
        currentlySelectedSpline = splines[index];
    }


    /// <summary> Adds a spline to the end of the list. </summary>
    public void AddSpline()
    {
        InsertSpline(splines.Count);
    }


    /// <summary> Inserts a spline at the given index. </summary>
    public void InsertSpline(int index)
    {
        T spline = CreateSpline();
        splines.Insert(index, spline);
        currentlySelectedSpline = spline;
    }


    /// <summary>
    /// Deletes the spline at the given index.
    /// If the selected spline is deleted, the first spline in the list is selected
    /// If the last spline is deleted, the selected spline is set to null
    /// </summary>
    public void DeleteSpline(int index)
    {
        T splineToDelete = splines[index];
        splines.RemoveAt(index);
        if (splineToDelete == currentlySelectedSpline)
        {
            if (splines.Count == 0)
            {
                currentlySelectedSpline = null;
            }
            else
            {
                currentlySelectedSpline = splines[0];
            }
        }
        Destroy(splineToDelete.gameObject);
    }


    T CreateSpline()
    {
        if (_splineParent == null)
            _splineParent = new GameObject(typeof(T).Name + " Collection");

        GameObject splineGA = new GameObject(typeof(T).Name + _splineCount);
        splineGA.transform.parent = _splineParent.transform;
        splineGA.layer = _splineParent.gameObject.layer;

        ++_splineCount;

        return splineGA.AddComponent<T>();
    }
}
