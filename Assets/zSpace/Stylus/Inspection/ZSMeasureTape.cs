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
/// Maintains a visualization of the start, end, span, and a text display for
/// a measurement tool.
/// </summary>
public class ZSMeasureTape : ZSMeasureWidget
{
  /// <summary>
  /// The current object's role in the tape.
  /// </summary>
  public enum Mode
  {
    /// <summary>
    /// Appears at the start of the measurement.
    /// </summary>
    Start = 0,

    /// <summary>
    /// Appears at the end of the measurement.
    /// </summary>
    End,

    /// <summary>
    /// Appears between the start and end of the measurement.
    /// Mesh must be locally aligned along the z axis from 0 to 1, the maximum measurement length.
    /// Must have a texture in UV 0 aligned along U from 0 to 1.
    /// </summary>
    Span,
  }


  /// <summary> What part of the widget are we maintaining? </summary>
  public Mode _mode = Mode.Span;

  /// <summary> Units to use if measuring distance. </summary>
  public DistanceUnit _distanceUnit = DistanceUnit.Meters;

  /// <summary> The material to use when drawing a metric tape measure. </summary>
  public Material _metricTapeMaterial;

  /// <summary> The material to use when drawing an imperial tape measure. </summary>
  public Material _imperialTapeMaterial;

  Camera _camera;
  float _initialLength;
  float _length = 1.0f;

  /// <summary>
  /// Scales the z component of each vertex in the object so it only reaches the given length.
  /// Scales u texture coordinate too, so the object's texture keeps the same proportion.
  /// </summary>
  float length
  {
    set
    {
      transform.localScale = new Vector3(1.0f, 1.0f, value / _initialLength);

      float scaleFactor = value / _length;
      if (_length == 1.0f)
        scaleFactor /= _initialLength;

      foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>(true))
      {
        if (_distanceUnit == DistanceUnit.Meters && meshFilter.renderer != null && meshFilter.renderer.material != _metricTapeMaterial)
          meshFilter.renderer.material = _metricTapeMaterial;
        else if (_distanceUnit == DistanceUnit.Imperial && meshFilter.renderer != null && meshFilter.renderer.material != _imperialTapeMaterial)
          meshFilter.renderer.material = _imperialTapeMaterial;

        Vector2[] uv = new Vector2[meshFilter.mesh.vertices.Length];
        for (int i = 0; i < meshFilter.mesh.vertices.Length; ++i)
        {
          uv[i] = meshFilter.mesh.uv[i];
          uv[i][0] *= scaleFactor;
        }
        meshFilter.mesh.uv = uv;
      }

      _length = value;
    }

    get { return _length; }
  }


  void Awake()
  {
    _camera = GameObject.Find("ZSCore").GetComponent<ZSCore>().CurrentCamera.camera;
  }


  void Start()
  {
    Bounds bounds = gameObject.transform.ComputeBounds();
    _initialLength = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
  }


  void LateUpdate()
  {
    // Set the transform.
    Vector3 forward = (Value.EndPoint - Value.StartPoint).normalized;
    Vector3 up = -_camera.transform.forward;
    transform.rotation = Quaternion.LookRotation(forward, up);

    if (_mode == Mode.Span)
    {
      transform.position = Value.StartPoint;

      // Clip the span object so it works like a measuring tape.
      length = Mathf.Max(1e-3f, Vector3.Distance(Value.EndPoint, Value.StartPoint));
    }
    else
    {
      transform.position = (_mode == Mode.Start) ? Value.StartPoint : Value.EndPoint;
    }
  }
}
