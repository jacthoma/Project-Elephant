////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Linq;
using UnityEngine;
using zSpace.Common;

/// <summary>
/// Animates the current GameObject along a spline after the user drops it nearby with the stylus.
/// </summary>
public class SplineAnimator : MonoBehaviour
{
  /// <summary>
  /// The speed in knots/s at which the target will animate along the spline.
  /// </summary>
  public float FlySpeed = 0.2f;

  /// <summary> The size of the area around a knot where the user must drag the object to start an animation. </summary>
  public float KnotRadius = 0.02f;

  /// <summary>
  /// If true, the object can begin animating at any knot in a spline and not just the first one.
  /// </summary>
  public bool SnapsToAnyKnot = true;

  /// <summary>
  /// Optional visual cue that animation is possible.
  /// </summary>
  /// <remarks>
  /// If this is set and the current GameObject comes close to a knot in a spline, the Hint
  /// GameObject will be activated and placed at the position and orientation of the knot,
  /// showing the user where the current GameObject would go before an animation would begin.
  /// This can be used to show a "ghost" version of the dragged object or a highlight on the spline.
  /// </remarks>
  public GameObject Hint;

  protected bool _isDragging = false;
  protected Bounds _bounds;
  protected Vector3 _boundsOffset = Vector3.zero;

  void Start()
  {
    //Compute the initial bounds and the relationship between them and the local origin.
    _bounds = gameObject.transform.ComputeBounds();
    _boundsOffset = _bounds.center - transform.position;
    _boundsOffset = Quaternion.Inverse(transform.rotation) * _boundsOffset;
    _boundsOffset = _boundsOffset.DivideComponents(transform.lossyScale);
  }

  void Update()
  {
    if (!_isDragging || Hint == null)
      return;

    int knotId = GetClosestKnotId();
    if (knotId >= 0)
    {
        Hint.SetActiveRecursively(true);

        SplineDrawer splineDrawer = SplineDrawer.GetInstance();
        Vector3 position = splineDrawer.GetKnotPosition(knotId);
        Hint.transform.position = position;

        if (knotId < splineDrawer.CurrentSplineKnotCount - 1)
        {
          Vector3 nextPosition = splineDrawer.GetKnotPosition(knotId + 1);
          Hint.transform.rotation = Quaternion.LookRotation(nextPosition - position);
        }
    }
    else if (Hint.active)
    {
        Hint.SetActiveRecursively(false);
    }
  }

  void OnZSDragToolBegin ()
  {
    if (animation != null)
      animation.Stop ();

    _isDragging = true;
  }

  protected int GetClosestKnotId()
  {
    SplineDrawer splineDrawer = SplineDrawer.GetInstance();
    Vector3 boundsOffset = transform.rotation * transform.lossyScale.MultiplyComponents(_boundsOffset);
    Vector3 boundsExtents = transform.lossyScale.MultiplyComponents(_bounds.extents);
    bool isOnSpline = splineDrawer.SelectClosestKnot(transform.position + boundsOffset, boundsExtents.magnitude + KnotRadius, true);
    //TODO: Check collision filters to prevent interaction with unintended splines.
    if (isOnSpline)
      return (SnapsToAnyKnot) ? splineDrawer.CurrentlySelectedKnotIndex : 0;

    return -1;
  }

  void OnZSDragToolEnd ()
  {
    _isDragging = false;

    if (Hint != null)
        Hint.SetActiveRecursively(false);

    int knotId = GetClosestKnotId();
    if (knotId != -1)
    {
      SplineDrawer splineDrawer = SplineDrawer.GetInstance();
      Vector3[] positions = new Vector3[splineDrawer.CurrentSplineKnotCount - knotId];
      for (int i = 0; i < positions.Length; ++i)
      {
        positions [i] = splineDrawer.GetKnotPosition(knotId + i);
        if (transform.parent != null)
          positions [i] = transform.parent.InverseTransformPoint(positions [i]);
      }

      //TODO: Use SplineSegment.GetNormal instead of Sloan method?
      Quaternion[] rotations = Utility.ComputeSplineRotations(positions, transform.up);

      float[] times = ComputeSplineTimes(positions, FlySpeed);

      transform.AnimateTo("localRotation", rotations, times);
      transform.AnimateTo("localPosition", positions, times);
    }
  }

  /// <summary>
  /// Computes keyframe times to ensure a constant speed along the given spline.
  /// </summary>
  /// <remarks>
  /// If the speed is 0 or omitted, it will be auto-computed so the animation completes in 1s.
  /// </remarks>
  public static float[] ComputeSplineTimes(Vector3[] positions, float speed = 0f)
  {
      bool isUnitLength = (speed == 0f);
      if (isUnitLength)
        speed = 1f;

      int frameCount = positions.Length;
      float[] times = new float[frameCount];
      times[0] = 0f;

      if (frameCount < 4)
      {
          for (int i = 1; i < frameCount; ++i)
              times[i] = times[i - 1] + (positions[i] - positions[i - 1]).magnitude / speed;
      }
      else
      {
          times[1] = (positions[1] - positions[0]).magnitude / speed;
    
          int samplesPerSegment = 16;
          Vector3 oldPosition = positions[1];
          for (int j = 2; j < frameCount - 1; ++j)
          {
              SplineSegment segment = new SplineSegment(positions[j-2], positions[j-1], positions[j], positions[j+1]);
              float segmentLength = 0f;
              for (int i = 0; i < samplesPerSegment; ++i)
              {
                  float t = (float)i / (float)(samplesPerSegment - 1);
                  Vector3 position = segment.GetPosition(t);
    
                  segmentLength += (position - oldPosition).magnitude;
    
                  oldPosition = position;
              }
    
              times[j] = times[j-1] + segmentLength / speed;
          }
    
          float endDistance = (positions[frameCount - 1] - positions[frameCount - 2]).magnitude;
          times[frameCount - 1] = times[frameCount - 2] + endDistance / speed;
      }

      if (isUnitLength)
      {
        float totalTime = 0f;
        foreach (float time in times)
            totalTime += time;

        for (int i = 0; i < times.Length; ++i)
            times[i] /= totalTime;
      }

      return times;
  }
}
