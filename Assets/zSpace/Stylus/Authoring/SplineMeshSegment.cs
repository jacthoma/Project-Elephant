////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zSpace.Common;

/// <summary> Defines a mesh for one Catmull-Rom spline segment. </summary>
public class SplineMeshSegment : MonoBehaviour
{
    /// <summary> Holds data about a point in the spline. </summary>
    protected class PointData
    {
        public Vector3 point;
        public Vector3 upVector;
        public Vector3 forwardVector;
    }

    public int SamplesPerSegment = 16;
    public int CylinderEdges = 16; 
    public float SplineRadius = .0018f;
    public Vector3 startingUpVector = Vector3.up;
    public Vector3 finalUpVector = Vector3.up;

    protected Mesh _mesh;
    protected List<PointData> _pointDataList = new List<PointData>();
    protected SplineSegment _segment;

    void Awake()
    {
        if (this.gameObject.GetComponent<MeshFilter>() == null)
        {
            MeshFilter meshFilter = this.gameObject.AddComponent<MeshFilter>();
            _mesh = new Mesh();
            _mesh.name = this.gameObject.name;
            meshFilter.sharedMesh = _mesh;
  
            this.gameObject.AddComponent<MeshRenderer>();
            renderer.material.color = Color.red;
  
            this.gameObject.AddComponent<MeshCollider>();
        }
    }


    /// <summary> Sets the material for the created mesh. </summary>
    public void SetMaterial(Material material)
    {
        renderer.material = material;
    }


    /// <summary> Creates the mesh given the 4 control points </summary>
    public void SetupSegment(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3)
    {
        _segment = new SplineSegment(P0, P1, P2, P3);
        SetupMesh();
    }


    protected void SetupMesh()
    {
        SetupPointDataList();

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        for(int i = 0; i < _pointDataList.Count; i++)
        {
            AddVertsFromPointData(verts, _pointDataList[i]);
            AddUVsFromPointData(uvs, i);
        }

        for (int i = 0; i < _pointDataList.Count - 1; i++)
        {
            AddTriangles(triangles, i, i + 1);
        }

        _mesh.vertices = verts.ToArray();
        _mesh.uv = uvs.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = _mesh;
    }


    protected void AddVertsFromPointData(List<Vector3> verts, PointData data)
    {
        for (int i = 0; i < CylinderEdges; i++)
        {
            Vector3 direction = Quaternion.AngleAxis((i * 1.0f / (CylinderEdges - 1)) * 360, data.forwardVector) * data.upVector;
            verts.Add(data.point + (direction * SplineRadius));
        }
    }


    protected void AddUVsFromPointData(List<Vector2> uvs, int index)
    {
        float depth = 1 - (index * 1.0f / (_pointDataList.Count - 1));
        for (int i = 0; i < CylinderEdges; i++)
        {
            float height = (i * 1.0f / (CylinderEdges - 1));
            Vector2 location = new Vector2(height, depth);
            uvs.Add(location);
        }
    }


    protected void AddTriangles(List<int> triangles, int startCylinderIndex, int endCylinderIndex)
    {
        //for each point on the cylinder, you need to make 2 triangles
        for (int i = 0; i < CylinderEdges; i++)
        {
            //first triangle has 2 on the start cylinder and 1 on the finish cylinder
            triangles.Add(GetTriangleIndex(GetWraparoundCylinderEdgeIndex(i), startCylinderIndex));
            triangles.Add(GetTriangleIndex(GetWraparoundCylinderEdgeIndex(i + 1), startCylinderIndex));
            triangles.Add(GetTriangleIndex(GetWraparoundCylinderEdgeIndex(i + 1), endCylinderIndex));
            //second trinagle has 1 on the start cylinder and 2 on the finish cylinder
            triangles.Add(GetTriangleIndex(GetWraparoundCylinderEdgeIndex(i), startCylinderIndex));
            triangles.Add(GetTriangleIndex(GetWraparoundCylinderEdgeIndex(i + 1), endCylinderIndex));
            triangles.Add(GetTriangleIndex(GetWraparoundCylinderEdgeIndex(i), endCylinderIndex));
        }
    }


    /// <summary> Returns the wraparound cylinder edge index. </summary>
    protected int GetWraparoundCylinderEdgeIndex(int CylinderEdgeIndex)
    {
        return CylinderEdgeIndex % CylinderEdges;
    }


    /// <summary> Converts the cylinder edge index into the triangle index. </summary>
    protected int GetTriangleIndex(int CylinderEdgeIndex, int CylinderIndex)
    {
        return CylinderEdges * CylinderIndex + CylinderEdgeIndex;
    }


    protected void SetupPointDataList()
    {
        _pointDataList.Clear();

        //Sample the spline and set the position at each sample.
        for (int i = 0; i < (SamplesPerSegment + 2); i++)
        {
            _pointDataList.Add(new PointData());
            //add a point for the start, end, and all of SamplesPerSegments
            _pointDataList[i].point = _segment.GetPosition((float)i / (float)(SamplesPerSegment + 1));
        }

        //Set the tangent at each sample.

        Vector3 fallbackTangent = (_segment.Points[3] - _segment.Points[1]).normalized;
        if (fallbackTangent == Vector3.zero)
            fallbackTangent = Vector3.up;

        for (int i = 0; i < _pointDataList.Count; i++)
        {
            float t = (float)i / (float)(_pointDataList.Count - 1);
            Vector3 tangent = _segment.GetTangent(t);
            if (tangent == Vector3.zero)
                tangent = fallbackTangent;
            _pointDataList[i].forwardVector = tangent;
        }

        //now setup the Up Vector
        _pointDataList[0].upVector = startingUpVector;
        for (int i = 1; i < _pointDataList.Count; i++)
        {
            float t = (float)i / (float)(_pointDataList.Count - 1);
            _pointDataList[i].upVector = _segment.GetNormal(t, _pointDataList[i - 1].upVector);
        }
        finalUpVector = _pointDataList[_pointDataList.Count - 1].upVector;
    }
}