////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using UnityEngine;
using zSpace.Common;

/// <summary>
/// Controls a waypoint in the camera's path.
/// </summary>
public class Waypoint : MonoBehaviour
{
    void Start()
    {
		foreach (Snap snap in transform.GetComponentsInChildren<Snap>(true))
			snap.objectResolver = x => x.transform.parent.parent.gameObject;
    }
}
