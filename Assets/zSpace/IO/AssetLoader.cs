////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

using System.Collections;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;
using zSpace.Common;

/// <summary>
/// Base class for loading GameObjects. Exposes interface and helpers for asynchronous runtime loading from file.
/// </summary>
/// <remarks>
/// @paragraph AssetLoaderSample Loading a File
/// 
/// To load an asset named "myFile.obj", you can use code like the following simplified code.
/// 
/// @code
/// class BackgroundLoader : MonoBehaviour
/// {
///   AssetLoader loader;
///   GameObject result;
///   AssetLoader.ColliderQuality quality = AssetLoader.ColliderQuality.Convex;
///   
///   void LoadFile(string fileName)
///   {
///     result = null;
///     loader = AssetLoader.Create(fileName);
///     loader.ColliderQuality = quality;
///     StartCoroutine(loader.LoadAssetFileCoroutine(fileName));
///   }
///   
///   void Update()
///   {
///     if (result == null && loader.LoadResult != null)
///       result = loader.LoadResult;
///   }
/// }
/// @endcode
/// 
/// It will move as much work as possible to a background thread,
/// making your application responsive during the load.
/// LoadAssetFileCoroutine also allows you to yield it from another coroutine. In that
/// case, LoadResult will be available as soon as control returns.
/// 
/// </remarks>
//TODO: Support non-coroutine loading in the foreground thread for simple use cases.
public abstract class AssetLoader : ZSUMonoBehavior
{
    /// <summary>
    /// Controls the Collider types that will be used, trading performance for quality.
    /// </summary>
    public enum ColliderQuality
    {
        Box = 0,
        Convex = 1,
        Concave = 2,
    }

    /// <summary>
    /// The default Cubemap to use for any reflection mapping.
    /// Typically this should match your skybox or other reflections in the scene.
    /// </summary>
    public Cubemap EnvironmentMap;

    /// <summary>
    /// If true, applicable materials will use a dynamic environment map with correct reflections.
    /// </summary>
    public bool UseDynamicEnvironmentMap = false;

    /// <summary> The maximum number of vertices per Mesh.  If a model exceeds this, it will have multiple sub-parts. </summary>
    public int VertexLimit = 32000;

    /// <summary> The maximum number of triangles per Mesh.  If a model exceeds this, it will have multiple sub-parts. </summary>
    public int FaceLimit = 32000;

    /// <summary> The material that will be used when none is assigned. </summary>
    public Material DefaultMaterial;

    /// <summary>
    /// If this hint is true, the scene and camera may require auto-fitting to accommodate the loaded GameObject.
    /// </summary>
    public bool IsAutoFittingEnabled = true;

    /// <summary>
    /// The type of collision geometry that will be added to the loaded GameObject.
    /// </summary>
    public ColliderQuality ColliderType = ColliderQuality.Box;

    /// <summary>
    /// The loaded GameObject.
    /// </summary>
    public GameObject LoadResult;

    /// <summary>
    /// A list of the file extensions that may be loaded by this loader.
    /// </summary>
    public static string[] SupportedFormats;


    public static IEnumerable<string> RegisteredFormats { get { return _loaderRegistry.Keys; } }
    private static Dictionary<string, List<Type>> _loaderRegistry = new Dictionary<string, List<Type>>();


    static AssetLoader()
    {
        var loaderTypes = new List<Type>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            loaderTypes.AddRange(assembly.GetTypes().Where(t =>
                t != typeof(AssetLoader) &&
                typeof(AssetLoader).IsAssignableFrom(t)));
        }

        Debug.Log("Found the following loaders:");

        foreach (var type in loaderTypes)
        {
            Debug.Log(type.Name);

            var supportedFormats = (string[])type.GetField("SupportedFormats").GetValue(null);

            if (supportedFormats == null || supportedFormats.Length == 0)
            {
                Debug.LogWarning("AssetLoader \"" + type.Name + "\" does not list any supported formats. It will not be used.");
                continue;
            }
            
            foreach (var formatUpper in supportedFormats)
            {
                var format = formatUpper.ToLower();
                if (!_loaderRegistry.ContainsKey(format))
                    _loaderRegistry[format] = new List<Type>();

                _loaderRegistry[format].Add(type);

                //Debug.Log("Added loader for " + format + ": " + type.Name);
            }
        }
    }


    /// <summary>
    /// Creates a suitable AssetLoader for loading the given file.
    /// </summary>
    /// <param name="hostObject">
    /// Loading will be performed asynchronously via Unity's Coroutine mechanism.
    /// The coroutine will be associated with this GameObject.
    /// </param>
    /// <param name="fileName">
    /// The name of the file that will be loaded. A suitable loader will be found for loading it.
    /// </param>
    /// <param name="order">
    /// One file format may be supported by multiple loaders. The compatible loaders are ordered alphanumerically by name.
    /// </param>
    /// <returns>
    /// The created AssetLoader, which can be configured and used to load the file.
    /// </returns>
    // In some cases, AssetLoader may prefer a specific loader over all others, breaking the usual ordering.
    // When this happens add documentation above.
    public static AssetLoader Create(GameObject hostObject, string fileName, int order = 0)
    {
        string format = Path.GetExtension(fileName).ToLower();

        if (!_loaderRegistry.ContainsKey(format))
        {
            Debug.LogWarning("Cannot find loader for " + format + " file.");
            return null;
        }

        var loaders = _loaderRegistry[format];
        Type type = (order < loaders.Count) ? loaders[order] : loaders.Last();

        Debug.Log("Using " + type.Name + " to load " + format + " file.");

        AssetLoader result = (AssetLoader)hostObject.AddComponent(type);
        return result;
    }

 
    /// <summary> Loads the specified file int a GameObject. </summary>
    /// <param name='fileName'> The file to load. </param>
    /// <exception cref="UnityException"> Thrown if there is a problem with the asset file or format. </exception>
    public IEnumerator LoadAssetFileCoroutine(string fileName)
    {
        LoadResult = null;
 
        if (!File.Exists(fileName))
            yield break;
   
        // Add Plugins directory to PATH so DLL dependencies can be delay-loaded.
        string pathValue = Environment.GetEnvironmentVariable("PATH");
        string pluginsDir = Application.dataPath + "/Plugins";
        if (!Regex.Match(pathValue, pluginsDir).Success)
        {
            string path = pluginsDir + ";" + pathValue;
            Environment.SetEnvironmentVariable("PATH", path);
        }

        yield return StartCoroutine(Load(fileName));
    }


    /// <summary>
    /// Loads the given file into the LoadResult GameObject. Override this to implement a custom loader.
    /// </summary>
    protected abstract IEnumerator Load(string fileName);

    /// <summary>
    /// Helper method that adds colliders to the given GameObject, based on quality settings.
    /// </summary>
    /// <remarks>
    /// May use box colliders, simplified mesh colliders, or convex decomosition-based mesh colliders.
    /// </remarks>
    protected IEnumerator AddColliders(GameObject go)
    {
        if (go == null)
            yield break;

        System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();

        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>(true);
        foreach (MeshFilter meshFilter in meshFilters)
        {
            GameObject partObject = meshFilter.gameObject;

            if (partObject.layer == LayerMask.NameToLayer("Default"))
            {
                if (partObject.collider == null)
                {
                    if (ColliderType == ColliderQuality.Box)
                    {
                        partObject.AddComponent<BoxCollider>();
                    }
                    else
                    {
                        // The collider quality is Convex or Concave. Compute a simplified collision mesh.

                        stopwatch1.Reset();
                        stopwatch1.Start();
                        int meshId = MeshProxy.ToProxy(meshFilter.mesh);
                        stopwatch1.Stop();
                        //Debug.Log("Exported proxy for mesh " + meshFilter.name + " in " + stopwatch1.ElapsedMilliseconds + "ms.");

                        int convexFaceLimit = 128;
                        int faceLimit = (ColliderType == ColliderQuality.Concave) ? 300 : convexFaceLimit;
                        if (MeshProxy.GetTriangleCount(meshId) > faceLimit)
                        {
                            stopwatch1.Reset();
                            stopwatch1.Start();
                            
                            yield return StartCoroutine(Utility.RunInBackground(() => MeshProxy.Simplify(meshId, faceLimit)));

                            stopwatch1.Stop();
                            //Debug.Log("Simplified mesh " + meshFilter.name + " in " + stopwatch1.ElapsedMilliseconds + "ms.");
                        }

                        int firstHullId = MeshProxy.GetMeshCount();
                        int convexHulls = 1;
                        if (ColliderType == ColliderQuality.Concave)
                        {
                            stopwatch1.Reset();
                            stopwatch1.Start();

                            yield return StartCoroutine(Utility.RunInBackground(() => convexHulls = MeshProxy.ConvexDecompose(meshId, convexFaceLimit)));

                            stopwatch1.Stop();
                            //Debug.Log("Decomposed mesh " + meshFilter.name + " into " + convexHulls + " convex hulls in " + stopwatch1.ElapsedMilliseconds + "ms.");
                        }

                        for (int i = 0; i < convexHulls; ++i)
                        {
                            int hullId = (i == 0) ? meshId : firstHullId + i - 1;
                            var mesh = MeshProxy.FromProxy(hullId);
                            if (mesh.IsDegenerate())
                            {
                                Debug.LogWarning("Using a box collider for degenerate mesh in " + partObject.name);
                                partObject.AddComponent<BoxCollider>();
                                continue;
                            }
                        }

                        for (int i = 0; i < convexHulls; ++i)
                        {
                            // Each convex hull has a separate GameObject at the same level as the original mesh.

                            int hullId = (i == 0) ? meshId : firstHullId + i - 1;

                            GameObject hullObject = new GameObject(partObject.name + "_hull" + i);
                            hullObject.active = false;
                            hullObject.layer = partObject.layer;
                            hullObject.transform.parent = partObject.transform.parent;
                            hullObject.transform.position = partObject.transform.position;
                            hullObject.transform.rotation = partObject.transform.rotation;
                            hullObject.transform.localScale = partObject.transform.localScale;

                            var mesh = MeshProxy.FromProxy(hullId);
                            MeshCollider hullCollider = hullObject.AddComponent<MeshCollider>();
                            hullCollider.convex = true;
                            hullCollider.smoothSphereCollisions = true;
                            hullCollider.sharedMesh = mesh;

                            MeshProxy.SetMesh(hullId, 0, 0);
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// Helper method that creates a Unity Mesh from the given attribute arrays.
    /// </summary>
    protected static Mesh[] CreateMesh(string name, int[] triangles, Vector3[] vertices, Vector3[] normals, Vector4[] tangents, Vector2[][] uv, Color[] colors, int vertexLimit)
    {
        if (triangles != null && triangles.Length < 3 || vertices == null || vertices.Length < 3)
            return new Mesh[] { };

        if (triangles == null)
            triangles = Enumerable.Range(0, vertices.Length).ToArray();

        //Debug.Log("Building Meshes for attribute arrays of " + vertices.Length + " vertices and " + triangles.Length/3 + " faces.");

        // The input will be split up into chunks, each with at most vertexLimit / 3 faces.

        vertexLimit -= vertexLimit % 3; //Make sure it's divisible by 3.
        var meshCount = (triangles.Length - 1) / vertexLimit + 1;
        var result = new Mesh[meshCount];

        var indexTable = new Dictionary<int, int>();
        var indexTableInv = new Dictionary<int, int>();

        for (int i = 0; i < result.Length; ++i)
        {
            result[i] = new Mesh();
            result[i].name = (result.Length == 1) ? name : name + i;

            // Re-index triangles to the minimum subset of vertices needed by this submesh.

            var trianglesChunk = triangles.Skip(i * vertexLimit).Take(vertexLimit).ToArray();
            var subTriangles = new int[trianglesChunk.Length];

            for (int j = 0; j < trianglesChunk.Length; ++j)
            {
                int index = trianglesChunk[j];
                if (!indexTable.ContainsKey(index))
                {
                    int subIndex = indexTableInv.Count;
                    indexTable[index] = subIndex;
                    indexTableInv[subIndex] = index;
                }
                subTriangles[j] = indexTable[index];
            }

            var subVertexCount = indexTable.Count;

            var subVertices = new Vector3[subVertexCount];
            for (int j = 0; j < subVertexCount; ++j)
                subVertices[j] = vertices[indexTableInv[j]];
            result[i].vertices = subVertices.ToArray();

            if (normals != null)
            {
                var subNormals = new Vector3[subVertexCount];
                for (int j = 0; j < subVertexCount; ++j)
                    subNormals[j] = normals[indexTableInv[j]];
                result[i].normals = subNormals;
            }

            if (tangents != null)
            {
                var subTangents = new Vector4[subVertexCount];
                for (int j = 0; j < subVertexCount; ++j)
                    subTangents[j] = tangents[indexTableInv[j]];
                result[i].tangents = subTangents;
            }

            if (uv.Length > 0 && uv[0] != null)
            {
                var subUv = new Vector2[subVertexCount];
                for (int j = 0; j < subVertexCount; ++j)
                    subUv[j] = uv[0][indexTableInv[j]];
                result[i].uv = subUv;
            }

            if (uv.Length > 1 && uv[1] != null)
            {
                var subUv1 = new Vector2[subVertexCount];
                for (int j = 0; j < subVertexCount; ++j)
                    subUv1[j] = uv[1][indexTableInv[j]];
                result[i].uv1 = subUv1;
            }

            if (colors != null)
            {
                var subColors = new Color[subVertexCount];
                for (int j = 0; j < subVertexCount; ++j)
                    subColors[j] = colors[indexTableInv[j]];
                result[i].colors = subColors;
            }

            result[i].triangles = subTriangles;

            indexTableInv.Clear();
            indexTable.Clear();
        }

        return result;
    }
}