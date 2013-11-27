////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Proxy for native meshing interfaces.  Simplifies interop for operations such as decimation and convex decomposition.
/// </summary>
public class MeshProxy
{
  #region ENUMERATIONS

  /// <summary>
  /// The type of attribute specified by attribute elements.
  /// </summary>
  public enum AttributeType
  {
    Position = 0,
    Normal = 1,
    Color = 2,
    UV0 = 3,
  }


  /// <summary>
  /// The type of face specified by face indices.
  /// </summary>
  public enum FaceType
  {
    Triangle = 0,
  }

  #endregion

  #region MeshProxy_APIS

  static int positionStride = 3;
  static int normalStride = 3;
  static int colorStride = 4;
  static int uv0Stride = 2;
  static int triangleStride = 3;

  ///<summary> Converts a Unity Mesh into a proxy for processing. </summary>
  public static int ToProxy(Mesh mesh)
  {
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    // Pick a slot for the mesh.
    int meshId = GetMeshCount();
    for (int i = 0; i < meshId; ++i)
    {
      if (GetVertexCount(i) == 0 && GetTriangleCount(i) == 0)
      {
        meshId = i;
        break;
      }
    }

    int vertexCount = mesh.vertices.Length;
    int triangleCount = mesh.triangles.Length / triangleStride;
    MeshProxy.SetMesh(meshId, vertexCount, triangleCount);

    float[] attributes = new float[ vertexCount * Mathf.Max(positionStride, Mathf.Max(normalStride, Mathf.Max(colorStride, uv0Stride) ) ) ];

    // Copy positions.
    {
      stopwatch.Reset();
      stopwatch.Start();

      Vector3[] vertices = mesh.vertices;
      for (int i = 0; i < vertices.Length; ++i)
      {
        for (int j = 0; j < positionStride; ++j)
          attributes[positionStride * i + j] = vertices[i][j];
      }
  
      MeshProxy.SetAttributes(meshId, (int)MeshProxy.AttributeType.Position, attributes);

      stopwatch.Stop();
      //Debug.Log("Copied " + mesh.name + " positions in " + stopwatch.ElapsedMilliseconds + "ms.");
    }
    
    // Copy normals.
    if (mesh.normals.Length > 0)
    {
      stopwatch.Reset();
      stopwatch.Start();

      Vector3[] normals = mesh.normals;
      for (int i = 0; i < normals.Length; ++i)
      {
        for (int j = 0; j < normalStride; ++j)
          attributes[normalStride * i + j] = normals[i][j];
      }
  
      MeshProxy.SetAttributes(meshId, (int)MeshProxy.AttributeType.Normal, attributes);

      stopwatch.Stop();
      //Debug.Log("Copied " + mesh.name + " normals in " + stopwatch.ElapsedMilliseconds + "ms.");
    }

    // Copy colors.
    if (mesh.colors.Length > 0)
    {
      stopwatch.Reset();
      stopwatch.Start();

      Color[] colors = mesh.colors;
      for (int i = 0; i < colors.Length; ++i)
      {
        for (int j = 0; j < colorStride; ++j)
          attributes[colorStride * i + j] = colors[i][j];
      }
  
      MeshProxy.SetAttributes(meshId, (int)MeshProxy.AttributeType.Color, attributes);

      stopwatch.Stop();
      //Debug.Log("Copied " + mesh.name + " colors in " + stopwatch.ElapsedMilliseconds + "ms.");
    }

    // Copy texture coordinates.
    if (mesh.uv.Length > 0)
    {
      stopwatch.Reset();
      stopwatch.Start();

      Vector2[] uv = mesh.uv;
      for (int i = 0; i < uv.Length; ++i)
      {
        for (int j = 0; j < uv0Stride; ++j)
          attributes[uv0Stride * i + j] = uv[i][j];
      }
  
      MeshProxy.SetAttributes(meshId, (int)MeshProxy.AttributeType.UV0, attributes);

      stopwatch.Stop();
      //Debug.Log("Copied " + mesh.name + " texture coordinates in " + stopwatch.ElapsedMilliseconds + "ms.");
    }

    // Copy triangles.

    stopwatch.Reset();
    stopwatch.Start();

    MeshProxy.SetFaces(meshId, (int)MeshProxy.FaceType.Triangle, mesh.triangles);

    stopwatch.Stop();
    //Debug.Log("Copied " + mesh.name + " triangles in " + stopwatch.ElapsedMilliseconds + "ms.");

    return meshId;
  }


  /// <summary>Converts a mesh proxy to a Unity Mesh.</summary>
  public static Mesh FromProxy(int meshId)
  {
    Mesh mesh = new Mesh();
    mesh.name = "MeshProxy" + meshId;

    int vertexCount = MeshProxy.GetVertexCount(meshId);
    int triangleCount = MeshProxy.GetTriangleCount(meshId);

    // Copy positions.
    {
      float[] floatPositions = new float[vertexCount * positionStride];
      MeshProxy.GetAttributes(meshId, (int)MeshProxy.AttributeType.Position, floatPositions);
      Vector3[] positions = new Vector3[vertexCount];

      for (int i = 0; i < vertexCount; ++i)
      {
        for (int j = 0; j < positionStride; ++j)
          positions[i][j] = floatPositions[positionStride * i + j];
      }
  
      mesh.vertices = positions;
    }

    // Copy normals.
    {
      if (MeshProxy.HasAttributes(meshId, (int)MeshProxy.AttributeType.Normal) != 0)
      {
        float[] floatNormals = new float[vertexCount * normalStride];
        MeshProxy.GetAttributes(meshId, (int)MeshProxy.AttributeType.Normal, floatNormals);
        Vector3[] normals = new Vector3[vertexCount];
    
        for (int i = 0; i < vertexCount; ++i)
        {
          for (int j = 0; j < normalStride; ++j)
            normals[i][j] = floatNormals[normalStride * i + j];
        }
    
        mesh.normals = normals;
      }
    }

    // Copy colors.
    {
      if (MeshProxy.HasAttributes(meshId, (int)MeshProxy.AttributeType.Color) != 0)
      {
        float[] floatColors = new float[vertexCount * colorStride];
        MeshProxy.GetAttributes(meshId, (int)MeshProxy.AttributeType.Color, floatColors);
        Color[] colors = new Color[vertexCount];

        for (int i = 0; i < vertexCount; ++i)
        {
          for (int j = 0; j < colorStride; ++j)
            colors[i][j] = floatColors[colorStride * i + j];
    
        }
    
        mesh.colors = colors;
      }
    }

    // Copy texture coordinates.
    {
      if (MeshProxy.HasAttributes(meshId, (int)MeshProxy.AttributeType.UV0) != 0)
      {
        float[] floatUv0 = new float[vertexCount * uv0Stride];
        MeshProxy.GetAttributes(meshId, (int)MeshProxy.AttributeType.UV0, floatUv0);
        Vector2[] uv0 = new Vector2[vertexCount];
    
        for (int i = 0; i < vertexCount; ++i)
        {
          for (int j = 0; j < uv0Stride; ++j)
            uv0[i][j] = floatUv0[uv0Stride * i + j];
        }
    
        mesh.uv = uv0;
      }
    }

    // Copy triangles.
    {
      int[] triangles = new int[triangleCount * triangleStride];
      MeshProxy.GetFaces(meshId, (int)MeshProxy.FaceType.Triangle, triangles);
      mesh.triangles = triangles;
    }

    return mesh;
  }

  #endregion

  #region MESH_PROXY_IMPORT_DECLARATIONS

  /// <summary>
  /// Adds, resets, or deletes the mesh at the given index.
  /// Clears the attribute and index buffers and reserves the requested amount space for position and triangle data.
  /// If vertexCount and triangleCount are 0, the mesh slot is effectively empty.
  /// </summary>
  [DllImport("ZSMeshProxy", EntryPoint="zsmp_setMesh")]
  public static extern void SetMesh(int meshId, int vertexCount, int triangleCount);

  /// <summary>
  /// Gets the number of mesh slots currently allocated.  Some of the mesh slots may be empty.
  /// </summary>
  [DllImport("ZSMeshProxy", EntryPoint="zsmp_getMeshCount")]
  public static extern int GetMeshCount();

  /// <summary>
  /// Gets the number of attributes in each attribute array.
  /// The number of floats in each array will be the attribute's stride times this.
  /// </summary>
  [DllImport("ZSMeshProxy", EntryPoint="zsmp_getVertexCount")]
  public static extern int GetVertexCount(int meshId);

  /// <summary>
  /// Gets the number of faces in each face array.
  /// The number of indices will be the face's stride times this.
  /// </summary>
  [DllImport("ZSMeshProxy", EntryPoint="zsmp_getTriangleCount")]
  public static extern int GetTriangleCount(int meshId);

  /// <summary>
  /// Returns 1 if the mesh at the given index has attributes of the specified type, else 0.
  /// </summary>
  [DllImport("ZSMeshProxy", EntryPoint="zsmp_hasAttributes")]
  public static extern int HasAttributes(int meshId, int attributeType);

  /// <summary>
  /// Replaces the attribute array of the given type in the given mesh with the given array.
  /// Assumes the attributeData array is populated with stride * attribute count floats.
  /// </summary>
  [DllImport("ZSMeshProxy", EntryPoint="zsmp_setAttributes")]
  public static extern void SetAttributes(int meshId, int attributeType, [In] float[] attributeData);

  /// <summary>
  /// Saves the attribute array of the given type from the given mesh to the given array.
  /// Assumes the attributeData array has allocated at least stride * attribute count floats.
  /// </summary>
  [DllImport("ZSMeshProxy", EntryPoint="zsmp_getAttributes")]
  public static extern void GetAttributes(int meshId, int attributeType, [Out] float[] attributeData);

  /// <summary>
  /// Returns 1 if the mesh at the given index has faces of the specified type, else 0.
  /// </summary>
  [DllImport("ZSMeshProxy", EntryPoint="zsmp_hasFaces")]
  public static extern int HasFaces(int meshId, int faceType);

  /// <summary>
  /// Replaces the face array of the given type in the given mesh with the given array.
  /// Assumes the indexData array is populated with stride * face count ints.
  /// </summary>
  [DllImport("ZSMeshProxy", EntryPoint="zsmp_setFaces")]
  public static extern void SetFaces(int meshId, int faceType, [In] int[] indexData);

  /// <summary>
  /// Saves the face array of the given type from the given mesh to the given array.
  /// Assumes the indexData array has allocated at least stride * face count ints.
  /// </summary>
  [DllImport("ZSMeshProxy", EntryPoint="zsmp_getFaces")]
  public static extern void GetFaces(int meshId, int faceType, [Out] int[] indexData);

  /// <summary>
  /// Decomposes the given (possibly concave) mesh into one or more convex pieces.
  /// The first convex piece will replace the original mesh.
  /// The rest will replace any empty mesh slots or be appended to the end of the mesh set.
  /// </summary>
  /// <returns>The number of convex pieces produced.</returns>
  [DllImport("ZSMeshProxy", EntryPoint="zsmp_convexDecompose")]
  public static extern int ConvexDecompose(int meshId, int targetFaceCount);

  /// <summary>
  /// Simplifies the specified mesh by reducing its face count to at most targetFaceCount.
  /// </summary>
  [DllImport("ZSMeshProxy", EntryPoint="zsmp_simplify")]
  public static extern void Simplify(int meshId, int targetFaceCount);

  #endregion
}
