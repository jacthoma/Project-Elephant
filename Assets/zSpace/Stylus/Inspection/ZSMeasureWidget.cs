////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Maintains MVC model information about a measurement tool. </summary>
public class ZSMeasureWidget : MonoBehaviour
{
    /// <summary> Holds the measurement data. </summary>
    public class Measurement
    {
        public Vector3 StartPoint = Vector3.zero;
        public Vector3 EndPoint = Vector3.zero;
        public Vector3 CornerPoint = Vector3.zero;
        public bool IsAngular = false;
    }


    /// <summary> Supported units of distance measurement. </summary>
    public enum DistanceUnit
    {
        Imperial = 0,
        Meters,
    }


    /// <summary> Supported units of angle measurement. </summary>
    public enum AngleUnit
    {
        Degrees = 0,
        Radians,
    }


    /// <summary> The measurement to visualize. </summary>
    public Measurement Value;
}
