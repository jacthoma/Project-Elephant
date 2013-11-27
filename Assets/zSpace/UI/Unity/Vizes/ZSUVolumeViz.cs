////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using zSpace.Common;
using zSpace.UI;

/// <summary>
/// Viz specializing in the display of voxel-based or point-cloud information.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class ZSUVolumeViz : ZSUViz
{
    public enum ValueMapping
    {
        Identity,
        Linear,
        Spline,
        Step,
    }

    public ValueMapping ValueMap
    {
        get { return _valueMap; }
        set
        {
            if (_valueMap == value)
                return;

            _valueMap = value;
            renderer.sharedMaterial.SetFloat("_ValueMap", (float)value);
        }
    }
    protected ValueMapping _valueMap = ValueMapping.Identity;

    public Vector4 ValueMapPoint0
    {
        get { return _points[0]; }
        set { SetPoint(0, value); }
    }
    public Vector4 ValueMapPoint1
    {
        get { return _points[1]; }
        set { SetPoint(1, value); }
    }
    public Vector4 ValueMapPoint2
    {
        get { return _points[2]; }
        set { SetPoint(2, value); }
    }
    public Vector4 ValueMapPoint3
    {
        get { return _points[3]; }
        set { SetPoint(3, value); }
    }
    protected Vector4[] _points = new Vector4[4] { Vector4.zero, Vector4.one, Vector4.zero, Vector4.one };

    protected override void OnScriptAwake()
    {
        base.OnScriptAwake();

        _mesh = GetComponentInChildren<MeshFilter>().sharedMesh;
        _renderer = GetComponentInChildren<MeshRenderer>();
        _texture = (Texture3D)_renderer.sharedMaterial.mainTexture;
        _colors = _texture.GetPixels();
    }

    /// <summary>
    /// Sets the ValueMapping control point at the given index to the given value.
    /// </summary>
    protected void SetPoint(int pointId, Vector4 point)
    {
        if (_points[pointId] == point)
            return;

        _points[pointId] = point;
        renderer.sharedMaterial.SetVector("_Point" + pointId, point);
    }


    /// <summary>
    /// Returns the density value (raw color with scaling) from the given viz's voxel grid at the given point.
    /// </summary>
    public Color GetDensity(Vector3 worldPoint)
    {
        var localPoint = transform.InverseTransformPoint(worldPoint);

        var offset = new Vector3(0.5f, 0.5f, 0.5f); //TODO: Auto-compute?
        var aspectRatio = _mesh.vertices[0].DivideComponents((Vector4)_mesh.colors[0] - (Vector4)offset);
        var uvw = localPoint.DivideComponents(aspectRatio) + offset;

        var size = new int[3] { _texture.width >> _mipLevel, _texture.height >> _mipLevel, _texture.depth >> _mipLevel };

        var ijk = new int[3];
        for (int i = 0; i < 3; ++i)
            ijk[i] = (int)(Mathf.Clamp01(uvw[i]) * (float)(size[i] - 1));

        int index = ijk[2] * size[1] * size[0] + ijk[1] * size[0] + ijk[0];

        return _colors[index] * renderer.sharedMaterial.GetFloat("_BaseDensity");
    }


    private static int _mipLevel = 0;
    private static MeshRenderer _renderer;
    private static Texture3D _texture;
    private static Color[] _colors;
    private static Mesh _mesh;
}
