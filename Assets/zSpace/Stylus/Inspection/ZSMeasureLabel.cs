////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zSpace.Common;

/// <summary>
/// Maintains a text display for a measurement tool.
/// </summary>
public class ZSMeasureLabel : ZSMeasureWidget
{
  /// <summary> Units to use if measuring distance. </summary>
  public DistanceUnit _distanceUnit = DistanceUnit.Meters;

  /// <summary> Units to use if measuring angle. </summary>
  public AngleUnit _angleUnit = AngleUnit.Degrees;

  /// <summary> The text box to update with a text display. </summary>
  public TextMesh _textMesh;

  /// <summary> The number of significant figures after the decimal point, if applicable. </summary>
  public int _numDigits = 2;

  public Camera _camera;

  void Awake()
  {
    _camera = GameObject.Find("ZSCore").GetComponent<ZSCore>().CurrentCamera.camera;
  }


  void LateUpdate()
  {
    // Set the transform.
    _textMesh.transform.rotation = Quaternion.LookRotation(_camera.transform.forward, _camera.transform.up);
    Vector3 position = (Value.StartPoint == Vector3.zero) ?
                         Value.CornerPoint :
                         (Value.StartPoint + Value.EndPoint) / 2.0f;

    float lookAtSpan = Vector3.Dot(Value.EndPoint - Value.StartPoint, _camera.transform.forward);
    position -= (0.5f * Mathf.Abs(lookAtSpan) + Utility.Epsilon) * _camera.transform.forward;

    _textMesh.transform.position = position;

    _textMesh.text = ComputeLabel();
  }


  /// <summary>
  /// Computes a measurement string based on the start and end points and current configuration.
  /// </summary>
  string ComputeLabel()
  {
    string measurement;
    if (Value.IsAngular)
    {
      Vector3 a = Value.StartPoint - Value.CornerPoint;
      Vector3 b = Value.EndPoint - Value.CornerPoint;
      float angle = Vector3.Angle(a, b);
      if (_angleUnit == AngleUnit.Degrees)
        measurement = String.Format("{0:F" + _numDigits + "}", angle) + " deg";
      else
        measurement = String.Format("{0:F" + _numDigits + "}", 0.0174532925 * angle) + " rad";
    }
    else
    {
      float distance = Vector3.Distance(Value.EndPoint, Value.StartPoint);
      if (_distanceUnit == DistanceUnit.Meters)
      {
        measurement = String.Format("{0:F" + _numDigits + "}", distance) + " m";
      }
      else
      {
        int miles = (int)(0.000621371f * distance);
        int feet = (int)(3.28084 * distance) - 5280 * miles;
        float inches = 39.3701f * distance - ((float)(12 * (5280 * miles + feet)));
        measurement = miles + " mi, " +
                      feet + " ft, " +
                      String.Format("{0:F" + _numDigits + "}", inches) + " in";
      }
    }

    return measurement;
  }
}
