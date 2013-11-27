////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maintains a visualization of the legs of an angle measured by a measurement tool.
/// </summary>
public class ZSMeasureAngle : ZSMeasureWidget
{
  /// <summary>
  /// The visualization for the line from the angle start to the corner.
  /// </summary>
  public LineRenderer _lineRendererAB;

  /// <summary>
  /// The visualization for the line from the corner to the angle end.
  /// </summary>
  public LineRenderer _lineRendererBC;

  /// <summary>
  /// The visualization for the arc from the angle start to the angle end.
  /// </summary>
  public LineRenderer _lineRendererAC;

  /// <summary>
  /// The visualization for the corner.
  /// </summary>
  public GameObject _cornerObject;

  void LateUpdate()
  {
    // Make sure the right sub-objects are active based on the measurement state.

    if (_cornerObject.active != (Value.CornerPoint != Vector3.zero))
      _cornerObject.SetActiveRecursively(!_cornerObject.active);

    if (_lineRendererAB.gameObject.active != (Value.StartPoint != Vector3.zero))
      _lineRendererAB.gameObject.SetActiveRecursively(!_lineRendererAB.gameObject.active);

    if (_lineRendererBC.gameObject.active != (Value.StartPoint != Vector3.zero))
      _lineRendererBC.gameObject.SetActiveRecursively(!_lineRendererBC.gameObject.active);

    if (_lineRendererAC.gameObject.active != (Value.StartPoint != Vector3.zero))
      _lineRendererAC.gameObject.SetActiveRecursively(!_lineRendererAC.gameObject.active);

    // Transform the objects based on the current measurement.

    if (Value.CornerPoint != Vector3.zero)
      _cornerObject.transform.position = Value.CornerPoint;

    if (Value.StartPoint != Vector3.zero)
    {
      _lineRendererAB.SetPosition(0, Value.StartPoint);
      _lineRendererAB.SetPosition(1, Value.CornerPoint);
      _lineRendererBC.SetPosition(0, Value.CornerPoint);
      _lineRendererBC.SetPosition(1, Value.EndPoint);

      Vector3 start = Value.StartPoint - Value.CornerPoint;
      Vector3 end = 0.5f * (Value.EndPoint - Value.CornerPoint);
      _cornerObject.transform.rotation = Quaternion.LookRotation(start, end);

      if (start.magnitude < end.magnitude)
        end *= start.magnitude / end.magnitude;
  
      Quaternion endRotation = Quaternion.FromToRotation(end, start);
      float angle = Vector3.Angle(start, end);
      int numSegments = (int)Mathf.Ceil(angle) + 1;
  
      _lineRendererAC.SetVertexCount(numSegments);
  
      for (int i = 0; i < numSegments; ++i)
      {
        float t = (float)i / angle;
        Vector3 offset = Quaternion.Slerp(endRotation, Quaternion.identity, t) * end;
        _lineRendererAC.SetPosition(i, Value.CornerPoint + offset);
      }
    }
  }
}
